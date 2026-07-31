using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation;

/// <summary>
/// KNOWLEDGE_MODEL §11. Falha bloqueia a conclusão de qualquer capacidade.
/// Verificados na própria compilação, não apenas em teste.
/// </summary>
public static class ModelInvariants
{
    public static IReadOnlyList<string> Validate(KnowledgeModel model)
    {
        var violations = new List<string>();

        var subjects = model.Observations.Select(o => o.Subject).ToHashSet();

        subjects.Add(model.Entities.Solution.Id);

        foreach (var project in model.Entities.Projects)
            subjects.Add(project.Id);

        foreach (var folder in model.Entities.Folders)
            subjects.Add(folder.Id);

        var observationIds = model.Observations.Select(o => o.Id).ToHashSet();
        var evidenceIds = model.Evidence.Select(e => e.Id).ToHashSet();

        ValidateObservations(model, subjects, violations);
        ValidateEvidence(model, observationIds, violations);
        ValidateInferences(model, subjects, evidenceIds, violations);

        return violations;
    }

    private static void ValidateObservations(
        KnowledgeModel model,
        HashSet<KnowledgeId> subjects,
        List<string> violations)
    {
        foreach (var observation in model.Observations)
        {
            var provenance = observation.Provenance;

            // IV-01
            if (string.IsNullOrWhiteSpace(provenance.Source)
                || string.IsNullOrWhiteSpace(provenance.Producer)
                || string.IsNullOrWhiteSpace(provenance.Capability))
            {
                violations.Add($"IV-01: proveniência incompleta em {observation.Id}");
            }

            // IV-03
            if (!subjects.Contains(observation.Subject))
                violations.Add($"IV-03: subject inexistente em {observation.Id}: {observation.Subject}");

            // IV-04
            if (!ObservationKinds.IsKnown(observation.Kind))
                violations.Add($"IV-04: kind fora do catálogo em {observation.Id}: {observation.Kind}");

            // IV-08
            foreach (var value in observation.Payload.Values.Values.Append(provenance.Source))
            {
                if (LooksAbsolute(value))
                    violations.Add($"IV-08: caminho absoluto em {observation.Id}: {value}");
            }
        }

        // IV-05
        var duplicated = model.Observations
            .GroupBy(o => o.Id)
            .Where(g => g
                .Select(o => o.Payload.ToCanonicalString())
                .Distinct(StringComparer.Ordinal)
                .Count() > 1)
            .Select(g => g.Key);

        foreach (var id in duplicated)
            violations.Add($"IV-05: id repetido com payloads divergentes: {id}");
    }

    private static void ValidateEvidence(
        KnowledgeModel model,
        HashSet<KnowledgeId> observationIds,
        List<string> violations)
    {
        foreach (var evidence in model.Evidence)
        {
            // IV-10
            if (evidence.Observations.Count == 0)
            {
                violations.Add($"IV-10: Evidence sem Observations: {evidence.Id}");
                continue;
            }

            if (!EvidenceKinds.IsKnown(evidence.Kind))
                violations.Add($"IV-04: kind de Evidence fora do catálogo: {evidence.Kind}");

            foreach (var observation in evidence.Observations)
            {
                if (!observationIds.Contains(observation))
                {
                    violations.Add(
                        $"IV-10: Evidence {evidence.Id} referencia Observation inexistente: {observation}");
                }
            }
        }
    }

    private static void ValidateInferences(
        KnowledgeModel model,
        HashSet<KnowledgeId> subjects,
        HashSet<KnowledgeId> evidenceIds,
        List<string> violations)
    {
        foreach (var inference in model.Inferences)
        {
            // IV-09
            if (!evidenceIds.Contains(inference.Evidence))
            {
                violations.Add(
                    $"IV-09: Inference {inference.Id} referencia Evidence inexistente: {inference.Evidence}");
            }

            // IV-03
            if (!subjects.Contains(inference.Subject))
                violations.Add($"IV-03: subject inexistente em {inference.Id}: {inference.Subject}");

            // IV-04
            if (!InferenceKinds.IsKnown(inference.Kind))
                violations.Add($"IV-04: kind de Inference fora do catálogo: {inference.Kind}");

            // IV-11
            switch (inference.Confidence)
            {
                case Confidence.Observed when inference.Frequency is null:
                    violations.Add($"IV-11: Inference {inference.Id} é Observed sem frequência");
                    break;

                case Confidence.Asserted when inference.Frequency is not null:
                    violations.Add($"IV-11: Inference {inference.Id} é Asserted com frequência");
                    break;
            }

            // IV-12
            if (string.IsNullOrWhiteSpace(inference.Provenance.Rule))
                violations.Add($"IV-12: Inference {inference.Id} sem regra declarada");

            if (string.IsNullOrWhiteSpace(inference.Provenance.Producer)
                || string.IsNullOrWhiteSpace(inference.Provenance.Capability))
            {
                violations.Add($"IV-01: proveniência incompleta em {inference.Id}");
            }
        }
    }

    private static bool LooksAbsolute(string value)
        => value.StartsWith('/')
           || value.Contains(":\\", StringComparison.Ordinal)
           || value.StartsWith("\\\\", StringComparison.Ordinal);
}

public sealed class InvariantViolationException(IReadOnlyList<string> violations)
    : InvalidOperationException(
        "Invariantes do KnowledgeModel violados:\n - " + string.Join("\n - ", violations))
{
    public IReadOnlyList<string> Violations { get; } = violations;
}
