using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// C05, fatia B — campos, eventos, operadores, indexadores, construtores
/// estáticos e implementações explícitas de interface (ADR-042).
/// </summary>
/// <remarks>
/// A solução de referência tem um indexador e nenhuma das outras formas. A
/// verificação desta fatia é contra a fixture, de propósito: o `Ledger`
/// declara todas elas.
/// </remarks>
public sealed class MemberFormsTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public MemberFormsTests(SolutionFixture fixture) => _fixture = fixture;

    private string NewOutput() => Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

    private Task<KnowledgeModel> CompileAsync(string? output = null)
        => KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output ?? NewOutput()).AsTask();

    private static bool IsSemantic(KnowledgeModel model)
        => model.Manifest.AcquisitionLevel == AcquisitionLevel.Semantic;

    private static IReadOnlyList<Observation> Declared(KnowledgeModel model, string kind)
        => model.Observations
            .Where(o => o.Kind == ObservationKinds.MemberDeclared && o.Payload["kind"] == kind)
            .ToArray();

    private static Observation InLedger(KnowledgeModel model, string kind, string name)
        => Declared(model, kind).Single(o =>
            o.Subject.Value.Contains(".Ledger.", StringComparison.Ordinal)
            && o.Payload["name"] == name);

    private static IReadOnlyList<Observation> About(KnowledgeModel model, string kind, KnowledgeId member)
        => model.Observations
            .Where(o => o.Kind == kind && o.Subject.Equals(member))
            .ToArray();

    [Fact]
    public async Task Invariantes_continuam_passando_com_as_formas_novas()
    {
        var model = await CompileAsync();

        Assert.Empty(ModelInvariants.Validate(model));
    }

    [Fact]
    public async Task Campo_const_registra_o_modificador_escrito()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var campo = InLedger(model, MemberVocabulary.Field, "Kind");

        var modificadores = About(model, ObservationKinds.MemberModifier, campo.Subject)
            .Select(o => o.Payload["name"]!)
            .ToArray();

        Assert.Contains("const", modificadores);

        // O valor é contrato: embutido no chamador em tempo de compilação,
        // trocá-lo quebra quem já compilou (ADR-044).
        var valor = Assert.Single(
            About(model, ObservationKinds.MemberConstantValue, campo.Subject));

        Assert.Equal("\"ledger\"", valor.Payload["value"]);

        // `const` é estático pela regra da linguagem, mas a declaração não
        // escreve `static`, e o que se observa é a declaração.
        Assert.DoesNotContain("static", modificadores);

        Assert.Single(About(model, ObservationKinds.MemberType, campo.Subject));
    }

    [Fact]
    public async Task Evento_em_forma_de_campo_nao_declara_acessor()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var campo = InLedger(model, MemberVocabulary.Event, "Changed");
        var comAcessores = InLedger(model, MemberVocabulary.Event, "Audited");

        Assert.Empty(About(model, ObservationKinds.MemberAccessor, campo.Subject));

        var acessores = About(model, ObservationKinds.MemberAccessor, comAcessores.Subject)
            .Select(o => o.Payload["kind"]!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "add", "remove" }, acessores);
    }

    /// <summary>
    /// Sem os tipos dos parâmetros, duas sobrecargas de indexador viram uma.
    /// </summary>
    [Fact]
    public async Task Indexador_leva_os_parametros_na_identidade_entre_colchetes()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var indexador = InLedger(model, MemberVocabulary.Indexer, "this");

        Assert.Contains("this[", indexador.Subject.Value, StringComparison.Ordinal);
        Assert.EndsWith("]@Domain", indexador.Subject.Value, StringComparison.Ordinal);

        Assert.Single(About(model, ObservationKinds.MemberParameter, indexador.Subject));
        Assert.Single(About(model, ObservationKinds.MemberType, indexador.Subject));
    }

    /// <summary>
    /// Identidade em forma de metadados, nome em forma de declaração.
    /// </summary>
    [Fact]
    public async Task Operador_tem_nome_declarado_e_identidade_de_metadados()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var soma = InLedger(model, MemberVocabulary.Operator, "+");

        Assert.Contains("op_Addition(", soma.Subject.Value, StringComparison.Ordinal);
        Assert.Equal(2, About(model, ObservationKinds.MemberParameter, soma.Subject).Count);

        var conversao = InLedger(model, MemberVocabulary.Operator, "implicit");

        Assert.Single(About(model, ObservationKinds.MemberType, conversao.Subject));
    }

    /// <summary>
    /// Construtor estático não ganhou espécie própria: é `constructor` com o
    /// modificador que a declaração escreve.
    /// </summary>
    [Fact]
    public async Task Construtor_estatico_e_construtor_com_modificador_static()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var estatico = Declared(model, MemberVocabulary.Constructor)
            .Single(o => o.Subject.Value.Contains(".Ledger..cctor(", StringComparison.Ordinal));

        Assert.Contains(
            About(model, ObservationKinds.MemberModifier, estatico.Subject),
            o => o.Payload["name"] == "static");

        Assert.Empty(About(model, ObservationKinds.MemberType, estatico.Subject));
    }

    [Fact]
    public async Task Implementacao_explicita_declara_a_interface_e_e_publicada()
    {
        var output = NewOutput();

        var model = await CompileAsync(output);

        if (!IsSemantic(model))
            return;

        var explicita = model.Observations
            .Single(o => o.Kind == ObservationKinds.MemberExplicitInterface);

        Assert.Equal("Reference.Domain.IAudit", explicita.Payload["interfaceName"]);
        Assert.NotNull(explicita.Payload["interfaceId"]);

        // A acessibilidade registrada é a dos metadados; quem decide que o
        // membro é superfície é a projeção, pela presença do fato acima.
        var acessibilidade = About(model, ObservationKinds.MemberAccessibility, explicita.Subject)
            .Single();

        Assert.Equal(TypeVocabulary.Private, acessibilidade.Payload["value"]);

        var conteudo = await File.ReadAllTextAsync(
            Path.Combine(output, "Behavior", "Domain", "Reference.Domain.Ledger.md"));

        Assert.Contains("Implementações explícitas", conteudo, StringComparison.Ordinal);
        Assert.Contains("IAudit.Record", conteudo, StringComparison.Ordinal);

        // A linguagem proíbe modificador de acesso ali; publicar `private`
        // seria publicar o que a declaração não diz.
        Assert.DoesNotContain("private void IAudit", conteudo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Projecao_publica_as_formas_novas_com_a_sintaxe_da_linguagem()
    {
        var output = NewOutput();

        var model = await CompileAsync(output);

        if (!IsSemantic(model))
            return;

        var conteudo = await File.ReadAllTextAsync(
            Path.Combine(output, "Behavior", "Domain", "Reference.Domain.Ledger.md"));

        Assert.Contains("public const string Kind = \"ledger\"", conteudo, StringComparison.Ordinal);
        Assert.Contains("public event EventHandler", conteudo, StringComparison.Ordinal);
        Assert.Contains("this[int index]", conteudo, StringComparison.Ordinal);
        Assert.Contains("operator +(", conteudo, StringComparison.Ordinal);
        Assert.Contains("implicit operator", conteudo, StringComparison.Ordinal);

        // Campo privado e construtor estático não são superfície.
        Assert.DoesNotContain("Limit", conteudo, StringComparison.Ordinal);
        Assert.DoesNotContain("static Ledger()", conteudo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Parametro_ref_readonly_produz_o_modificador_do_vocabulario()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var apply = Declared(model, MemberVocabulary.Method)
            .Single(o => o.Subject.Value.Contains(".Ledger.Apply(", StringComparison.Ordinal));

        var modificadores = About(model, ObservationKinds.MemberParameter, apply.Subject)
            .Select(o => o.Payload["modifier"])
            .ToArray();

        Assert.Contains("ref-readonly", modificadores);
        Assert.Contains("in", modificadores);
    }

    /// <summary>
    /// `default` e `null` são a mesma coisa nos metadados e coisas
    /// diferentes na declaração. O que se publica é o que está escrito.
    /// </summary>
    [Fact]
    public async Task Valor_padrao_e_registrado_como_escrito()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var apply = Declared(model, MemberVocabulary.Method)
            .Single(o => o.Subject.Value.Contains(".Ledger.Apply(", StringComparison.Ordinal));

        var padroes = About(model, ObservationKinds.MemberParameter, apply.Subject)
            .Where(o => o.Payload["optional"] == "true")
            .ToDictionary(o => o.Payload["name"]!, o => o.Payload["defaultValue"]);

        Assert.Equal("\"x\"", padroes["rotulo"]);
        Assert.Equal("null", padroes["escala"]);
        Assert.Equal("default", padroes["nota"]);
    }
}
