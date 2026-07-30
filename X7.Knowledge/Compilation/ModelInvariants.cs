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

        var ids = model.Observations.Select(o => o.Subject).ToHashSet();

        ids.Add(model.Entities.Solution.Id);

        foreach (var project in model.Entities.Projects)
            ids.Add(project.Id);

        foreach (var folder in model.Entities.Folders)
            ids.Add(folder.Id);

        foreach (var observation in model.Observations)
        {
            // IV-01
            var provenance = observation.Provenance;

            if (string.IsNullOrWhiteSpace(provenance.Source)
                || string.IsNullOrWhiteSpace(provenance.Producer)
                || string.IsNullOrWhiteSpace(provenance.Capability))
            {
                violations.Add($"IV-01: proveniência incompleta em {observation.Id}");
            }

            // IV-03
            if (!ids.Contains(observation.Subject))
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
            .Where(g => g.Select(o => o.Payload.ToCanonicalString()).Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(g => g.Key);

        foreach (var id in duplicated)
            violations.Add($"IV-05: id repetido com payloads divergentes: {id}");

        return violations;
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
