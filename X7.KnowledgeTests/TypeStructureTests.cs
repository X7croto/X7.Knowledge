using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// C04 — classificação, acessibilidade, modificadores, parâmetros genéricos e
/// aninhamento.
/// </summary>
/// <remarks>
/// Nenhum teste aqui fixa o conjunto de kinds, de tipos ou de capacidades:
/// todo teste que fez isso quebrou na capacidade seguinte. Verificam-se
/// formato e propriedade.
/// </remarks>
public sealed class TypeStructureTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public TypeStructureTests(SolutionFixture fixture) => _fixture = fixture;

    private Task<KnowledgeModel> CompileAsync()
        => KnowledgeCompiler.CompileAsync(
            _fixture.SolutionPath,
            Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"))).AsTask();

    /// <summary>
    /// Localiza pelo nome simples. Em nível S a identidade de um genérico
    /// traz a lista de parâmetros — `...IEvents&lt;TIn, TOut&gt;@Domain` —,
    /// então casar apenas com `.Nome@` acharia zero e o teste passaria vazio
    /// em vez de falhar.
    /// </summary>
    private static IReadOnlyList<Observation> Of(KnowledgeModel model, string kind, string typeName)
        => model.Observations
            .Where(o => o.Kind == kind && Matches(o.Subject, typeName))
            .ToArray();

    private static bool Matches(KnowledgeId subject, string typeName)
        => subject.Value.Contains($".{typeName}@", StringComparison.Ordinal)
           || subject.Value.Contains($".{typeName}<", StringComparison.Ordinal);

    private static IEnumerable<Observation> Types(KnowledgeModel model)
        => model.Observations.Where(o => o.Kind == ObservationKinds.TypeDeclared);

    [Fact]
    public async Task Todo_tipo_declara_classificacao_e_acessibilidade_uma_unica_vez()
    {
        var model = await CompileAsync();

        // É IV-14. Verificado aqui também porque é o critério 1 do C04:
        // sem ele, "representação completa" seria julgamento subjetivo.
        Assert.Empty(ModelInvariants.Validate(model));

        foreach (var type in Types(model))
        {
            Assert.Single(model.Observations.Where(o =>
                o.Kind == ObservationKinds.TypeKind && o.Subject.Equals(type.Subject)));

            Assert.Single(model.Observations.Where(o =>
                o.Kind == ObservationKinds.TypeAccessibility && o.Subject.Equals(type.Subject)));
        }
    }

    [Fact]
    public async Task Classificacao_e_acessibilidade_ficam_no_vocabulario()
    {
        var model = await CompileAsync();

        Assert.All(
            model.Observations.Where(o => o.Kind == ObservationKinds.TypeKind),
            o => Assert.True(TypeVocabulary.IsKnownKind(o.Payload["kind"]!)));

        Assert.All(
            model.Observations.Where(o => o.Kind == ObservationKinds.TypeAccessibility),
            o => Assert.True(TypeVocabulary.IsKnownAccessibility(o.Payload["value"]!)));

        Assert.All(
            model.Observations.Where(o => o.Kind == ObservationKinds.TypeModifier),
            o => Assert.True(TypeVocabulary.IsKnownModifier(o.Payload["name"]!)));
    }

    [Fact]
    public async Task Record_struct_nao_e_colapsado_em_struct()
    {
        var model = await CompileAsync();

        var kind = Assert.Single(Of(model, ObservationKinds.TypeKind, "Money"));

        Assert.Equal(TypeVocabulary.RecordStruct, kind.Payload["kind"]);
    }

    [Fact]
    public async Task Modificador_implicito_de_metadados_nao_e_publicado()
    {
        var model = await CompileAsync();

        // Toda interface é abstrata em metadados e ninguém escreveu isso.
        // Publicar seria o mesmo problema de observar que tudo deriva de
        // Object: uma Observation por tipo, informando nada.
        Assert.Empty(Of(model, ObservationKinds.TypeModifier, "IRepository"));

        var abstratos = Of(model, ObservationKinds.TypeModifier, "RepositoryBase");

        Assert.Contains(abstratos, o => o.Payload["name"] == "abstract");
    }

    [Fact]
    public async Task Classe_estatica_declara_static_e_nao_abstract_sealed()
    {
        var model = await CompileAsync();

        var modificadores = Of(model, ObservationKinds.TypeModifier, "Clock")
            .Select(o => o.Payload["name"])
            .ToArray();

        Assert.Contains("static", modificadores);
        Assert.DoesNotContain("abstract", modificadores);
        Assert.DoesNotContain("sealed", modificadores);
    }

    [Fact]
    public async Task Parametro_generico_declara_ordinal_e_variancia()
    {
        var model = await CompileAsync();

        var parametros = Of(model, ObservationKinds.TypeGenericParameter, "IEvents")
            .OrderBy(o => int.Parse(o.Payload["ordinal"]!))
            .ToArray();

        Assert.Equal(2, parametros.Length);

        Assert.Equal("TIn", parametros[0].Payload["name"]);
        Assert.Equal("in", parametros[0].Payload["variance"]);

        Assert.Equal("TOut", parametros[1].Payload["name"]);
        Assert.Equal("out", parametros[1].Payload["variance"]);

        // Invariante nunca escreve `variance`: campo ausente é ausência,
        // nunca a string "null".
        var invariante = Of(model, ObservationKinds.TypeGenericParameter, "IQuery");

        Assert.NotEmpty(invariante);
        Assert.All(invariante, o => Assert.Null(o.Payload["variance"]));
    }

    [Fact]
    public async Task Tipo_aninhado_aponta_o_contentor_e_sai_do_namespace()
    {
        var model = await CompileAsync();

        var aninhado = Assert.Single(Of(model, ObservationKinds.TypeNestedIn, "Line"));

        var containerId = KnowledgeId.Parse(aninhado.Payload["containerId"]!);

        Assert.Contains(model.Observations, o =>
            o.Kind == ObservationKinds.TypeDeclared && o.Subject.Equals(containerId));

        // O namespace contém o contentor, não o aninhado: dois caminhos até o
        // mesmo tipo fariam a hierarquia deixar de ser árvore.
        var conteudo = model.Observations
            .Where(o => o.Kind == ObservationKinds.NamespaceContains)
            .Select(o => o.Payload["typeId"]!)
            .ToArray();

        Assert.Contains(containerId.Value, conteudo);
        Assert.DoesNotContain(aninhado.Subject.Value, conteudo);
    }

    [Fact]
    public async Task Tipo_parcial_e_inferido_de_multiplos_locais_com_evidence()
    {
        var model = await CompileAsync();

        var inference = Assert.Single(model.Inferences.Where(i =>
            i.Kind == InferenceKinds.TypeIsPartial
            && i.Subject.Value.Contains(".Catalog@", StringComparison.Ordinal)));

        Assert.Equal(Confidence.Asserted, inference.Confidence);
        Assert.Null(inference.Frequency);

        var evidence = Assert.Single(model.Evidence.Where(e => e.Id.Equals(inference.Evidence)));

        // IV-17: um local só não sustenta conclusão sobre parcialidade.
        Assert.True(evidence.Observations.Count >= 2);

        // Tipo não parcial não recebe a Inference.
        Assert.DoesNotContain(model.Inferences, i =>
            i.Kind == InferenceKinds.TypeIsPartial
            && i.Subject.Value.Contains(".Money@", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Limite_da_regra_de_parcialidade_e_declarado()
    {
        var model = await CompileAsync();

        // Ausência de `type.is-partial` significa não detectado, nunca não
        // parcial. Sem esta limitação a Base afirmaria mais do que sabe.
        Assert.Contains(model.Observations, o =>
            o.Kind == ObservationKinds.AcquisitionLimitation
            && o.Payload["affectedScope"] == "type-partial-single-site");
    }

    [Fact]
    public async Task Tipo_parcial_registra_todos_os_locais_de_declaracao()
    {
        var model = await CompileAsync();

        var locais = Of(model, ObservationKinds.TypeLocation, "Catalog")
            .Select(o => o.Payload["file"]!)
            .ToArray();

        // Conjunto, não sequência: OB-04 ordena Observations de mesmo subject
        // e mesmo kind por id, que é hash de conteúdo. Esperar ordem de
        // arquivo aqui seria testar uma garantia que o modelo não dá — quem
        // ordena por arquivo é o Publisher, na hora de exibir.
        Assert.Equal(2, locais.Length);
        Assert.Equal(2, locais.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("src/Domain/Catalog.cs", locais);
        Assert.Contains("src/Domain/Catalog.Extra.cs", locais);
    }

    [Fact]
    public async Task Projecao_de_tipos_seciona_por_classificacao()
    {
        var output = Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        var domain = await File.ReadAllTextAsync(
            Path.Combine(output, "Structure", "Types", "Domain.md"));

        Assert.Contains("## interface", domain, StringComparison.Ordinal);
        Assert.Contains("## class", domain, StringComparison.Ordinal);

        // Nome curto com os parâmetros reconstruídos, uma vez só.
        Assert.Contains("| IEvents<TIn, TOut> |", domain, StringComparison.Ordinal);
        Assert.DoesNotContain("Reference.Domain.IEvents", domain, StringComparison.Ordinal);

        // Relação de tipo continua fora do inventário (§9.1, ADR-035).
        Assert.DoesNotContain("Herda de", domain, StringComparison.Ordinal);
    }
}
