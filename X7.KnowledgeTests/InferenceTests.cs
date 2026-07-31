using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// Extensão v0.2: Evidence, Inference, Confidence.
/// Nenhum Producer produz esses itens ainda — os kinds estão reservados
/// para C02. Estes testes exercitam o mecanismo, não uma capacidade.
/// </summary>
public sealed class InferenceTests
{
    private static readonly Provenance ObservationProvenance = new()
    {
        Source = "Reference.slnx",
        Producer = "TestProducer",
        Capability = "C02",
        AcquisitionLevel = AcquisitionLevel.Syntactic
    };

    private static readonly InferenceProvenance Rule = new()
    {
        Rule = "layer-by-graph-depth",
        Producer = "TestProducer",
        Capability = "C02",
        AcquisitionLevel = AcquisitionLevel.Syntactic
    };

    private static Observation AnyObservation(string name = "A")
        => Observation.Create(
            ObservationKinds.ProjectDeclared,
            KnowledgeId.ForProject($"src/{name}/{name}.csproj"),
            ObservationPayload.From(
                ("name", name),
                ("relativePath", $"src/{name}/{name}.csproj"),
                ("directory", $"src/{name}")),
            ObservationProvenance);

    private static Evidence AnyEvidence(params Observation[] observations)
        => Evidence.Create(
            EvidenceKinds.ProjectGraphPosition,
            observations.Select(o => o.Id),
            "TestProducer",
            "C02");

    // --- Evidence ---

    [Fact]
    public void Evidence_sem_observations_e_rejeitada()
    {
        var erro = Assert.Throws<InvalidOperationException>(() =>
            Evidence.Create(EvidenceKinds.ProjectGraphPosition, [], "P", "C02"));

        Assert.Contains("sem Observations", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Evidence_com_mesmas_observations_tem_mesmo_id()
    {
        var a = AnyObservation("A");
        var b = AnyObservation("B");

        var primeira = AnyEvidence(a, b);
        var segunda = AnyEvidence(b, a);

        Assert.Equal(primeira.Id, segunda.Id);
    }

    [Fact]
    public void Evidence_ordena_observations_independente_da_entrada()
    {
        var a = AnyObservation("A");
        var b = AnyObservation("B");

        var evidence = AnyEvidence(b, a);

        Assert.Equal(
            evidence.Observations.OrderBy(o => o).ToArray(),
            evidence.Observations.ToArray());
    }

    [Fact]
    public void Kind_de_evidence_fora_do_catalogo_falha()
    {
        Assert.Throws<UnknownEvidenceKindException>(() =>
            Evidence.Create("inventado", [AnyObservation().Id], "P", "C02"));
    }

    // --- Inference ---

    [Fact]
    public void Inference_asserted_nao_admite_frequencia()
    {
        var evidence = AnyEvidence(AnyObservation());

        var erro = Assert.Throws<InvalidOperationException>(() =>
            Inference.Create(
                InferenceKinds.ProjectIsRoot,
                KnowledgeId.ForProject("src/A/A.csproj"),
                ObservationPayload.Empty,
                evidence,
                Confidence.Asserted,
                Rule,
                new Frequency(3, 4)));

        Assert.Contains("não admite frequência", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Inference_observed_exige_frequencia()
    {
        var evidence = AnyEvidence(AnyObservation());

        var erro = Assert.Throws<InvalidOperationException>(() =>
            Inference.Create(
                InferenceKinds.ProjectLayer,
                KnowledgeId.ForProject("src/A/A.csproj"),
                ObservationPayload.From(("layer", "domain")),
                evidence,
                Confidence.Observed,
                Rule));

        Assert.Contains("exige frequência declarada", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Inference_sem_regra_declarada_falha()
    {
        var evidence = AnyEvidence(AnyObservation());

        var semRegra = Rule with { Rule = "   " };

        Assert.Throws<InvalidOperationException>(() =>
            Inference.Create(
                InferenceKinds.ProjectIsLeaf,
                KnowledgeId.ForProject("src/A/A.csproj"),
                ObservationPayload.Empty,
                evidence,
                Confidence.Asserted,
                semRegra));
    }

    [Fact]
    public void Inference_aponta_sempre_para_sua_evidence()
    {
        var evidence = AnyEvidence(AnyObservation());

        var inference = Inference.Create(
            InferenceKinds.ProjectIsRoot,
            KnowledgeId.ForProject("src/A/A.csproj"),
            ObservationPayload.Empty,
            evidence,
            Confidence.Asserted,
            Rule);

        Assert.Equal(evidence.Id, inference.Evidence);
        Assert.StartsWith("inf:", inference.Id.Value, StringComparison.Ordinal);
    }

    // --- Frequency ---

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(-1, 5)]
    [InlineData(6, 5)]
    public void Frequencia_invalida_e_rejeitada(int matching, int total)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new Frequency(matching, total));

    [Theory]
    [InlineData(117, 120, 975)]
    [InlineData(1, 3, 333)]
    [InlineData(5, 5, 1000)]
    public void Taxa_em_milesimos_e_inteira(int matching, int total, int expected)
        => Assert.Equal(expected, new Frequency(matching, total).RatePerMille);

    // --- Builder ---

    [Fact]
    public void Builder_recusa_evidence_com_observation_ausente()
    {
        var builder = new KnowledgeModelBuilder();

        var orfa = AnyEvidence(AnyObservation());

        var erro = Assert.Throws<InvalidOperationException>(() => builder.AddEvidence(orfa));

        Assert.Contains("Observation inexistente", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Builder_recusa_inference_com_evidence_ausente()
    {
        var builder = new KnowledgeModelBuilder();

        var observation = builder.Add(AnyObservation());
        var evidence = AnyEvidence(observation);

        var inference = Inference.Create(
            InferenceKinds.ProjectIsRoot,
            observation.Subject,
            ObservationPayload.Empty,
            evidence,
            Confidence.Asserted,
            Rule);

        // Evidence nunca foi registrada no builder.
        var erro = Assert.Throws<InvalidOperationException>(() => builder.AddInference(inference));

        Assert.Contains("Evidence inexistente", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Cadeia_completa_registra_e_ordena()
    {
        var builder = new KnowledgeModelBuilder();

        var solution = Observation.Create(
            ObservationKinds.SolutionDeclared,
            KnowledgeId.ForSolution("Reference"),
            ObservationPayload.From(("name", "Reference")),
            ObservationProvenance);

        builder.Add(solution);

        var a = builder.Add(AnyObservation("A"));
        var b = builder.Add(AnyObservation("B"));

        var evidence = builder.AddEvidence(AnyEvidence(a, b));

        builder.AddInference(Inference.Create(
            InferenceKinds.ProjectLayer,
            a.Subject,
            ObservationPayload.From(("layer", "domain")),
            evidence,
            Confidence.Observed,
            Rule,
            new Frequency(2, 3)));

        var model = builder.Build("0.2.0", "test", AcquisitionLevel.Syntactic, ["C02"], "digest");

        Assert.Equal(1, model.Manifest.EvidenceCount);
        Assert.Equal(1, model.Manifest.InferenceCount);
        Assert.Single(model.Inferences);
        Assert.Equal(evidence.Id, model.Inferences[0].Evidence);
    }
}
