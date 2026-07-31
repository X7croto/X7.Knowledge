using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

public sealed class ModelTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public ModelTests(SolutionFixture fixture) => _fixture = fixture;

    private Task<KnowledgeModel> CompileAsync()
        => KnowledgeCompiler.CompileAsync(
            _fixture.SolutionPath,
            Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"))).AsTask();

    [Fact]
    public async Task Invariantes_passam_na_solucao_de_referencia()
    {
        var model = await CompileAsync();

        Assert.Empty(ModelInvariants.Validate(model));
    }

    [Fact]
    public async Task Toda_observation_tem_proveniencia_completa()
    {
        var model = await CompileAsync();

        Assert.All(model.Observations, o =>
        {
            Assert.False(string.IsNullOrWhiteSpace(o.Provenance.Source));
            Assert.False(string.IsNullOrWhiteSpace(o.Provenance.Producer));
            // Agnóstico à capacidade de propósito: o modelo é aditivo (EX-01)
            // e fixar a lista faria este teste quebrar a cada capacidade nova.
            Assert.Matches("^C[0-9]{2}$", o.Provenance.Capability);
        });
    }

    [Fact]
    public async Task Todo_campo_de_entidade_e_rastreavel_a_uma_observation()
    {
        var model = await CompileAsync();

        foreach (var project in model.Entities.Projects)
        {
            Assert.Contains(model.Observations, o =>
                o.Kind == ObservationKinds.ProjectDeclared
                && o.Subject.Equals(project.Id)
                && o.Payload["name"] == project.Name);

            foreach (var framework in project.TargetFrameworks)
            {
                Assert.Contains(model.Observations, o =>
                    o.Kind == ObservationKinds.ProjectTargetFramework
                    && o.Subject.Equals(project.Id)
                    && o.Payload["moniker"] == framework);
            }
        }
    }

    [Fact]
    public async Task Observations_estao_ordenadas_por_subject_kind_id()
    {
        var model = await CompileAsync();

        var expected = model.Observations
            .OrderBy(o => o.Subject)
            .ThenBy(o => o.Kind, StringComparer.Ordinal)
            .ThenBy(o => o.Id)
            .Select(o => o.Id)
            .ToArray();

        Assert.Equal(expected, model.Observations.Select(o => o.Id).ToArray());
    }

    [Fact]
    public async Task Pasta_aninhada_preserva_hierarquia()
    {
        var model = await CompileAsync();

        var core = model.Entities.Folders.Single(f => f.Id.Value == "slnfolder:src/Core");

        Assert.Equal("Core", core.Name);
        Assert.Equal("slnfolder:src", core.Parent?.Value);
    }

    [Fact]
    public async Task Projeto_fora_de_pasta_permanece_na_raiz()
    {
        var model = await CompileAsync();

        var foldered = model.Entities.Folders.SelectMany(f => f.Children).ToHashSet();

        var cli = model.Entities.Projects.Single(p => p.Name == "Cli");

        Assert.DoesNotContain(cli.Id, foldered);
    }

    [Fact]
    public async Task Multi_target_produz_uma_observation_por_framework()
    {
        var model = await CompileAsync();

        var domain = model.Entities.Projects.Single(p => p.Name == "Domain");

        Assert.Equal(["net10.0", "net9.0"], domain.TargetFrameworks);
    }

    [Fact]
    public async Task Projeto_de_teste_e_detectado_com_evidencia()
    {
        var model = await CompileAsync();

        var tests = model.Entities.Projects.Single(p => p.Name == "Domain.Tests");

        Assert.True(tests.IsTestProject);

        var observation = model.Observations.Single(o =>
            o.Kind == ObservationKinds.ProjectIsTestProject && o.Subject.Equals(tests.Id));

        Assert.Equal("package:xunit", observation.Payload["evidence"]);
    }

    [Fact]
    public async Task Propriedade_nao_resolvida_vira_limitacao_declarada()
    {
        var model = await CompileAsync();

        var kernel = model.Entities.Projects.Single(p => p.Name == "Kernel");

        // Um projeto pode acumular limitações de várias capacidades.
        // O teste é sobre a de propriedade não resolvida, especificamente.
        var limitation = model.Observations.Single(o =>
            o.Kind == ObservationKinds.AcquisitionLimitation
            && o.Subject.Equals(kernel.Id)
            && o.Payload["affectedScope"] == "project-property");

        Assert.Equal("project-property", limitation.Payload["affectedScope"]);
        Assert.Contains("AssemblyName", limitation.Payload["reason"]!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Nivel_de_aquisicao_do_C01_e_sintatico()
    {
        var model = await CompileAsync();

        Assert.Equal(AcquisitionLevel.Syntactic, model.Manifest.AcquisitionLevel);
        Assert.All(model.Observations, o =>
            Assert.Equal(AcquisitionLevel.Syntactic, o.Provenance.AcquisitionLevel));
    }

    [Fact]
    public void Sentinela_de_kind_invalido_nunca_entra_no_catalogo()
        => Assert.False(ObservationKinds.IsKnown("kind.que.nunca.existira"));

    [Fact]
    public void Kind_fora_do_catalogo_falha_a_compilacao()
    {
        Assert.Throws<UnknownObservationKindException>(() =>
            Observation.Create(
                // Sentinela que nunca entrará no catálogo. Usar um kind
                // plausível fez este teste quebrar quando C03 o tornou válido.
                "kind.que.nunca.existira",
                KnowledgeId.ForSolution("X"),
                ObservationPayload.Empty,
                new Provenance
                {
                    Source = "X.slnx",
                    Producer = "T",
                    Capability = "C01",
                    AcquisitionLevel = AcquisitionLevel.Syntactic
                }));
    }

    [Fact]
    public void Observations_identicas_deduplicam_pelo_id()
    {
        var builder = new KnowledgeModelBuilder();

        var provenance = new Provenance
        {
            Source = "X.slnx",
            Producer = "T",
            Capability = "C01",
            AcquisitionLevel = AcquisitionLevel.Syntactic
        };

        var id = KnowledgeId.ForSolution("X");

        builder.Add(ObservationKinds.SolutionDeclared, id,
            ObservationPayload.From(("name", "X")), provenance);

        builder.Add(ObservationKinds.SolutionDeclared, id,
            ObservationPayload.From(("name", "X")), provenance);

        Assert.Single(builder.Observations);
    }
}
