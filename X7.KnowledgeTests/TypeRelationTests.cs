using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>C04 — herança e implementação. Exige nível S.</summary>
public sealed class TypeRelationTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public TypeRelationTests(SolutionFixture fixture) => _fixture = fixture;

    private Task<KnowledgeModel> CompileAsync()
        => KnowledgeCompiler.CompileAsync(
            _fixture.SolutionPath,
            Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"))).AsTask();

    private static bool IsSemantic(KnowledgeModel model)
        => model.Manifest.AcquisitionLevel == AcquisitionLevel.Semantic;

    private static IReadOnlyList<Observation> Of(KnowledgeModel model, string kind, string typeName)
        => model.Observations
            .Where(o => o.Kind == kind && o.Subject.Value.Contains($".{typeName}@", StringComparison.Ordinal))
            .ToArray();

    [Fact]
    public async Task Invariantes_continuam_passando_com_C04()
    {
        var model = await CompileAsync();

        Assert.Empty(ModelInvariants.Validate(model));
        Assert.Contains("C04", model.Manifest.Capabilities);
    }

    [Fact]
    public async Task Heranca_dentro_da_solucao_referencia_identidade_existente()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var inherits = Assert.Single(Of(model, ObservationKinds.TypeInherits, "OrderRepository"));

        Assert.Equal("Reference.Domain.RepositoryBase", inherits.Payload["baseTypeName"]);
        Assert.NotNull(inherits.Payload["baseTypeId"]);

        Assert.Contains(model.Observations, o =>
            o.Kind == ObservationKinds.TypeDeclared
            && o.Subject.Value == inherits.Payload["baseTypeId"]);
    }

    [Fact]
    public async Task Heranca_externa_registra_nome_e_se_declara_externa()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var inherits = Assert.Single(Of(model, ObservationKinds.TypeInherits, "DomainError"));

        Assert.Equal("System.Exception", inherits.Payload["baseTypeName"]);
        Assert.Equal("true", inherits.Payload["external"]);
        Assert.Null(inherits.Payload["baseTypeId"]);
    }

    [Fact]
    public async Task Base_implicita_nao_vira_observation()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        // Order é uma classe simples: deriva de Object, e isso não informa nada.
        Assert.Empty(Of(model, ObservationKinds.TypeInherits, "Order"));

        Assert.DoesNotContain(model.Observations, o =>
            o.Kind == ObservationKinds.TypeInherits
            && o.Payload["baseTypeName"] == "System.Object");
    }

    [Fact]
    public async Task Apenas_interfaces_declaradas_diretamente_sao_observadas()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        // OrderRepository declara IDisposable; IRepository vem de RepositoryBase
        // e é derivável, não observável.
        var implementa = Of(model, ObservationKinds.TypeImplements, "OrderRepository")
            .Select(o => o.Payload["interfaceName"])
            .ToArray();

        Assert.Contains("System.IDisposable", implementa);
        Assert.DoesNotContain("Reference.Domain.IRepository", implementa);
    }

    [Fact]
    public async Task Implementacao_dentro_da_solucao_referencia_identidade_existente()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var implementa = Assert.Single(Of(model, ObservationKinds.TypeImplements, "RepositoryBase"));

        Assert.Equal("Reference.Domain.IRepository", implementa.Payload["interfaceName"]);
        Assert.NotNull(implementa.Payload["interfaceId"]);
    }

    [Fact]
    public async Task Generico_construido_aponta_para_a_declaracao()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var implementa = Assert.Single(Of(model, ObservationKinds.TypeImplements, "NameQuery"));

        // Nome mostra o uso; identidade aponta a declaração.
        Assert.Contains("List<string>", implementa.Payload["interfaceName"]!, StringComparison.Ordinal);

        var alvo = implementa.Payload["interfaceId"];

        Assert.NotNull(alvo);
        Assert.Contains("IQuery<T>", alvo!, StringComparison.Ordinal);

        Assert.Contains(model.Observations, o =>
            o.Kind == ObservationKinds.TypeDeclared && o.Subject.Value == alvo);
    }

    [Fact]
    public async Task Relacoes_declaram_nivel_semantico()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        Assert.All(
            model.Observations.Where(o =>
                o.Kind is ObservationKinds.TypeInherits or ObservationKinds.TypeImplements),
            o =>
            {
                Assert.Equal(AcquisitionLevel.Semantic, o.Provenance.AcquisitionLevel);
                Assert.Equal("C04", o.Provenance.Capability);
            });
    }

    [Fact]
    public async Task Nivel_X_declara_limitacao_e_nao_produz_relacao()
    {
        var model = await CompileAsync();

        if (IsSemantic(model))
            return;

        Assert.Empty(model.Observations.Where(o =>
            o.Kind is ObservationKinds.TypeInherits or ObservationKinds.TypeImplements));

        Assert.Contains(model.Observations, o =>
            o.Kind == ObservationKinds.AcquisitionLimitation
            && o.Payload["affectedScope"] == "type-relations");
    }

    [Fact]
    public async Task Relacoes_ficam_fora_do_inventario_de_tipos()
    {
        var output = Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

        var model = await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        if (!IsSemantic(model))
            return;

        var inventario = await File.ReadAllTextAsync(
            Path.Combine(output, "Structure", "Types", "Domain.md"));

        var relacoes = await File.ReadAllTextAsync(
            Path.Combine(output, "Relations", "Domain.md"));

        // Quem pergunta "onde está o tipo X" não deve pagar pelas relações.
        Assert.DoesNotContain("Herda de", inventario, StringComparison.Ordinal);
        Assert.Contains("OrderRepository", inventario, StringComparison.Ordinal);

        Assert.Contains("Herda de", relacoes, StringComparison.Ordinal);
        Assert.Contains("RepositoryBase", relacoes, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compilacoes_repetidas_continuam_byte_identicas_com_C04()
    {
        var first = Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));
        var second = Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, first);
        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, second);

        foreach (var file in Directory.EnumerateFiles(first, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(first, file);

            Assert.True(
                File.ReadAllBytes(file).SequenceEqual(
                    File.ReadAllBytes(Path.Combine(second, relative))),
                $"Arquivo divergente: {relative}");
        }
    }
}
