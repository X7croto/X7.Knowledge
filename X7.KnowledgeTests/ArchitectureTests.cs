using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>C02 — dependências, grafo, camadas, raízes, folhas e ciclos.</summary>
public sealed class ArchitectureTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public ArchitectureTests(SolutionFixture fixture) => _fixture = fixture;

    private Task<KnowledgeModel> CompileAsync()
        => KnowledgeCompiler.CompileAsync(
            _fixture.SolutionPath,
            Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"))).AsTask();

    private static KnowledgeId Project(string relativePath)
        => KnowledgeId.ForProject(relativePath);

    private static int Depth(KnowledgeModel model, string name)
    {
        var project = model.Entities.Projects.Single(p => p.Name == name);

        var inference = model.Inferences.Single(i =>
            i.Kind == InferenceKinds.ProjectLayer && i.Subject.Equals(project.Id));

        return int.Parse(inference.Payload["depth"]!);
    }

    private static bool Has(KnowledgeModel model, string kind, string name)
    {
        var project = model.Entities.Projects.Single(p => p.Name == name);

        return model.Inferences.Any(i => i.Kind == kind && i.Subject.Equals(project.Id));
    }

    [Fact]
    public async Task Invariantes_continuam_passando_com_inferences()
    {
        var model = await CompileAsync();

        Assert.Empty(ModelInvariants.Validate(model));
        Assert.True(model.Manifest.InferenceCount > 0);
        Assert.True(model.Manifest.EvidenceCount > 0);
    }

    [Fact]
    public async Task Referencia_declarada_vira_observation()
    {
        var model = await CompileAsync();

        var kernel = Project("src/Core/Kernel/Kernel.csproj");
        var domain = Project("src/Domain/Domain.csproj");

        Assert.Contains(model.Observations, o =>
            o.Kind == ObservationKinds.ProjectReferencesProject
            && o.Subject.Equals(kernel)
            && o.Payload["targetId"] == domain.Value);
    }

    [Fact]
    public async Task Nenhuma_referencia_inventada_aparece()
    {
        var model = await CompileAsync();

        // A fixture declara exatamente três referências entre projetos.
        var references = model.Observations
            .Count(o => o.Kind == ObservationKinds.ProjectReferencesProject);

        Assert.Equal(3, references);
    }

    [Fact]
    public async Task Profundidade_reflete_posicao_no_grafo()
    {
        var model = await CompileAsync();

        Assert.Equal(0, Depth(model, "Domain"));
        Assert.Equal(1, Depth(model, "Kernel"));
        Assert.Equal(1, Depth(model, "Domain.Tests"));
        Assert.Equal(2, Depth(model, "Cli"));
    }

    [Fact]
    public async Task Raiz_e_folha_sao_identificadas()
    {
        var model = await CompileAsync();

        Assert.True(Has(model, InferenceKinds.ProjectIsRoot, "Cli"));
        Assert.True(Has(model, InferenceKinds.ProjectIsRoot, "Domain.Tests"));
        Assert.False(Has(model, InferenceKinds.ProjectIsRoot, "Domain"));

        Assert.True(Has(model, InferenceKinds.ProjectIsLeaf, "Domain"));
        Assert.False(Has(model, InferenceKinds.ProjectIsLeaf, "Cli"));
    }

    [Fact]
    public async Task Solucao_sem_ciclo_nao_produz_inference_de_ciclo()
    {
        var model = await CompileAsync();

        Assert.DoesNotContain(model.Inferences, i =>
            i.Kind == InferenceKinds.ProjectParticipatesInCycle);
    }

    [Fact]
    public async Task Toda_inference_aponta_evidence_existente_e_regra()
    {
        var model = await CompileAsync();

        var evidenceIds = model.Evidence.Select(e => e.Id).ToHashSet();

        Assert.All(model.Inferences, i =>
        {
            Assert.Contains(i.Evidence, evidenceIds);
            Assert.False(string.IsNullOrWhiteSpace(i.Provenance.Rule));
            Assert.Equal("C02", i.Provenance.Capability);
        });
    }

    [Fact]
    public async Task Pacote_declarado_vira_observation_com_versao()
    {
        var model = await CompileAsync();

        var tests = Project("tests/Domain.Tests/Domain.Tests.csproj");

        var observation = model.Observations.Single(o =>
            o.Kind == ObservationKinds.ProjectPackageReference && o.Subject.Equals(tests));

        Assert.Equal("xunit", observation.Payload["name"]);
        Assert.Equal("2.9.3", observation.Payload["version"]);
    }

    [Fact]
    public async Task Projecao_arquitetural_e_publicada()
    {
        var output = Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        var architecture = Path.Combine(output, "Architecture", "Architecture.md");
        var dependencies = Path.Combine(output, "Architecture", "ProjectDependencies.md");

        Assert.True(File.Exists(architecture));
        Assert.True(File.Exists(dependencies));

        var content = await File.ReadAllTextAsync(architecture);

        Assert.Contains("Camada 0", content, StringComparison.Ordinal);
        Assert.Contains("Camada 2", content, StringComparison.Ordinal);
        Assert.Contains("layer-by-graph-depth", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compilacoes_repetidas_continuam_byte_identicas_com_C02()
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
