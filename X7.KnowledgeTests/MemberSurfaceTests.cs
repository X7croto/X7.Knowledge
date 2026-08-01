using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;
using X7.Knowledge.Publishing;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// C05, fatia A — superfície declarada de métodos, construtores e
/// propriedades. Exige nível S.
/// </summary>
/// <remarks>
/// Nenhum teste fixa o conjunto de kinds, de membros ou de capacidades: todo
/// teste que fez isso quebrou na capacidade seguinte. Verificam-se formato e
/// propriedade.
/// </remarks>
public sealed class MemberSurfaceTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public MemberSurfaceTests(SolutionFixture fixture) => _fixture = fixture;

    private string NewOutput() => Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

    private Task<KnowledgeModel> CompileAsync(string? output = null)
        => KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output ?? NewOutput()).AsTask();

    private static bool IsSemantic(KnowledgeModel model)
        => model.Manifest.AcquisitionLevel == AcquisitionLevel.Semantic;

    private static IReadOnlyList<Observation> Members(KnowledgeModel model)
        => model.Observations
            .Where(o => o.Kind == ObservationKinds.MemberDeclared)
            .ToArray();

    private static Observation Member(KnowledgeModel model, string typeName, string memberName)
        => Members(model).Single(o =>
            o.Subject.Value.Contains($".{typeName}.{memberName}", StringComparison.Ordinal));

    private static IReadOnlyList<Observation> About(
        KnowledgeModel model,
        string kind,
        KnowledgeId member)
        => model.Observations
            .Where(o => o.Kind == kind && o.Subject.Equals(member))
            .ToArray();

    [Fact]
    public async Task Invariantes_continuam_passando_com_C05()
    {
        var model = await CompileAsync();

        Assert.Empty(ModelInvariants.Validate(model));
        Assert.Contains("C05", model.Manifest.Capabilities);
    }

    [Fact]
    public async Task Nivel_sintatico_declara_limitacao_em_vez_de_deduzir()
    {
        var model = await CompileAsync();

        if (IsSemantic(model))
        {
            Assert.NotEmpty(Members(model));

            return;
        }

        Assert.Empty(Members(model));

        Assert.Contains(model.Observations, o =>
            o.Kind == ObservationKinds.AcquisitionLimitation
            && o.Payload["affectedScope"] == "type-members");
    }

    /// <summary>
    /// O que a fatia não cobre é ausência declarada, nunca silenciosa.
    /// </summary>
    [Fact]
    public async Task Fatia_incompleta_declara_a_propria_limitacao()
    {
        var model = await CompileAsync();

        Assert.Contains(model.Observations, o =>
            o.Kind == ObservationKinds.AcquisitionLimitation
            && o.Payload["affectedScope"] == "type-members-partial");
    }

    [Fact]
    public async Task Todo_membro_declara_especie_do_vocabulario_e_uma_acessibilidade()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var members = Members(model);

        Assert.NotEmpty(members);

        Assert.All(members, o => Assert.True(MemberVocabulary.IsKnownKind(o.Payload["kind"]!)));

        foreach (var member in members)
        {
            Assert.Single(About(model, ObservationKinds.MemberAccessibility, member.Subject));

            Assert.All(
                About(model, ObservationKinds.MemberAccessibility, member.Subject),
                o => Assert.True(TypeVocabulary.IsKnownAccessibility(o.Payload["value"]!)));
        }
    }

    [Fact]
    public async Task Todo_membro_pertence_a_exatamente_um_tipo_existente()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var types = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeDeclared)
            .Select(o => o.Subject)
            .ToHashSet();

        foreach (var member in Members(model))
        {
            var containers = model.Observations
                .Where(o => o.Kind == ObservationKinds.TypeDeclaresMember
                            && o.Payload["memberId"] == member.Subject.Value)
                .ToArray();

            Assert.Single(containers);
            Assert.Contains(containers[0].Subject, types);
        }
    }

    /// <summary>
    /// Sobrecargas só se distinguem pelos tipos de parâmetro. Se a identidade
    /// os ignorasse, as duas viravam uma e o modelo perdia um membro em
    /// silêncio.
    /// </summary>
    [Fact]
    public async Task Sobrecargas_produzem_identidades_distintas()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var sobrecargas = Members(model)
            .Where(o => o.Payload["name"] == "Add")
            .Select(o => o.Subject.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(2, sobrecargas.Length);
    }

    [Fact]
    public async Task Propriedade_nao_tem_parenteses_na_identidade_e_metodo_tem()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        foreach (var member in Members(model))
        {
            var identidade = member.Subject.Value;

            if (member.Payload["kind"] == MemberVocabulary.Property)
                Assert.DoesNotContain("(", identidade, StringComparison.Ordinal);
            else
                Assert.Contains("(", identidade, StringComparison.Ordinal);
        }
    }

    /// <summary>IV-19: construtor não escreve tipo na declaração.</summary>
    [Fact]
    public async Task Construtor_nao_declara_tipo_e_metodo_declara()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        foreach (var member in Members(model))
        {
            var tipos = About(model, ObservationKinds.MemberType, member.Subject);

            if (member.Payload["kind"] == MemberVocabulary.Constructor)
                Assert.Empty(tipos);
            else
                Assert.Single(tipos);
        }
    }

    [Fact]
    public async Task Parametros_registram_ordinal_modificador_e_opcionalidade()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var tryFind = Member(model, "Order", "TryFind");

        var parametros = About(model, ObservationKinds.MemberParameter, tryFind.Subject);

        Assert.Equal(2, parametros.Count);

        Assert.Contains(parametros, p => p.Payload["modifier"] == "out" && p.Payload["name"] == "found");

        Assert.Equal(
            new[] { 0, 1 },
            parametros.Select(p => int.Parse(p.Payload["ordinal"]!)).Order().ToArray());

        var opcional = Members(model)
            .Where(o => o.Payload["name"] == "Add")
            .SelectMany(o => About(model, ObservationKinds.MemberParameter, o.Subject))
            .Where(p => p.Payload["optional"] == "true")
            .ToArray();

        Assert.Single(opcional);
    }

    [Fact]
    public async Task Metodo_generico_registra_parametro_de_tipo()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var map = Member(model, "Order", "Map");

        var parametro = Assert.Single(
            About(model, ObservationKinds.MemberGenericParameter, map.Subject));

        Assert.Equal("T", parametro.Payload["name"]);
        Assert.Equal("0", parametro.Payload["ordinal"]);

        // `T` não é tipo declarado da solução: vira nome e marca de externo,
        // nunca uma identidade que IV-13 não encontraria.
        var retorno = Assert.Single(About(model, ObservationKinds.MemberType, map.Subject));

        Assert.Equal("T", retorno.Payload["typeName"]);
        Assert.Null(retorno.Payload["typeId"]);
    }

    [Fact]
    public async Task Propriedade_registra_acessores_declarados()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var number = Member(model, "Order", "Number");

        var acessores = About(model, ObservationKinds.MemberAccessor, number.Subject)
            .Select(o => o.Payload["kind"]!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "get", "init" }, acessores);
    }

    /// <summary>
    /// Membro implícito é derivável da regra da linguagem. Observá-lo
    /// encheria a Base de conteúdo que o leitor já sabe — o argumento das
    /// bases implícitas do C04.
    /// </summary>
    [Fact]
    public async Task Membro_gerado_pela_linguagem_nao_e_observado()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var nomes = Members(model).Select(o => o.Payload["name"]!).ToArray();

        Assert.DoesNotContain("get_Number", nomes);
        Assert.DoesNotContain("<Clone>$", nomes);
        Assert.DoesNotContain("PrintMembers", nomes);
    }

    [Fact]
    public async Task Membro_nao_publico_existe_no_modelo_e_nao_na_projecao()
    {
        var output = NewOutput();

        var model = await CompileAsync(output);

        if (!IsSemantic(model))
            return;

        Assert.Contains(Members(model), o => o.Payload["name"] == "Recalculate");

        var arquivo = Path.Combine(output, "Behavior", "Domain", "Reference.Domain.Order.md");

        Assert.True(File.Exists(arquivo), arquivo);

        var conteudo = await File.ReadAllTextAsync(arquivo);

        Assert.Contains("Add", conteudo, StringComparison.Ordinal);
        Assert.Contains("protected virtual int Total", conteudo, StringComparison.Ordinal);
        Assert.DoesNotContain("Recalculate", conteudo, StringComparison.Ordinal);
        Assert.DoesNotContain("Secret", conteudo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Nome_de_arquivo_deriva_da_identidade_do_tipo()
    {
        var output = NewOutput();

        var model = await CompileAsync(output);

        if (!IsSemantic(model))
            return;

        // Aninhamento vira `+`, que não é válido em identificador C# e por
        // isso nunca colide com um tipo de nome parecido.
        var aninhado = Path.Combine(
            output, "Behavior", "Domain", "Reference.Domain.Order+Line.md");

        Assert.True(File.Exists(aninhado), aninhado);
    }

    [Fact]
    public async Task Indice_de_behavior_nunca_lista_nomes_de_tipo()
    {
        var output = NewOutput();

        var model = await CompileAsync(output);

        if (!IsSemantic(model))
            return;

        var indice = await File.ReadAllTextAsync(
            Path.Combine(output, "Behavior", "INDEX.md"));

        Assert.DoesNotContain("OrderRepository", indice, StringComparison.Ordinal);
        Assert.DoesNotContain("IOrderPolicy", indice, StringComparison.Ordinal);
        Assert.Contains("Domain", indice, StringComparison.Ordinal);
    }

    /// <summary>
    /// O layout por projeto existe só para a medição da ADR-040. Verificado
    /// aqui porque medição que não roda não mede nada.
    /// </summary>
    [Fact]
    public async Task Layout_de_medicao_publica_um_arquivo_por_projeto()
    {
        var output = NewOutput();

        var model = await KnowledgeCompiler.CompileAsync(
            _fixture.SolutionPath,
            output,
            until: null,
            behaviorLayout: BehaviorLayout.PerProject);

        if (!IsSemantic(model))
            return;

        Assert.True(File.Exists(Path.Combine(output, "Behavior", "Domain.md")));

        Assert.False(Directory.Exists(Path.Combine(output, "Behavior", "Domain")));
    }

    /// <summary>
    /// D-08. Duas compilações da mesma entrada produzem saída byte-idêntica.
    /// </summary>
    [Fact]
    public async Task Compilacoes_repetidas_produzem_behavior_identico()
    {
        var first = NewOutput();
        var second = NewOutput();

        await CompileAsync(first);
        await CompileAsync(second);

        var raiz = Path.Combine(first, "Behavior");

        if (!Directory.Exists(raiz))
            return;

        foreach (var file in Directory.EnumerateFiles(raiz, "*", SearchOption.AllDirectories))
        {
            var espelho = Path.Combine(second, Path.GetRelativePath(first, file));

            Assert.True(File.Exists(espelho), espelho);

            Assert.True(
                File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(espelho)),
                $"Saída divergente em {Path.GetRelativePath(first, file)}");
        }
    }
}
