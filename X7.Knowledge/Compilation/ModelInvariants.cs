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
        ValidateTypeStructure(model, violations);
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

        // IV-13: referência a tipo dentro do payload precisa existir no
        // modelo. Sem isso, uma relação apontaria para o vazio e a Base
        // pareceria completa enquanto não é.
        var typeIds = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeDeclared)
            .Select(o => o.Subject.Value)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var observation in model.Observations)
        {
            foreach (var key in (string[])["baseTypeId", "interfaceId", "containerId"])
            {
                var reference = observation.Payload[key];

                if (reference is not null && !typeIds.Contains(reference))
                    violations.Add($"IV-13: {observation.Id} referencia tipo inexistente: {reference}");
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

    /// <summary>
    /// IV-14 a IV-16. IV-14 é o que torna testável o critério 1 do C04 —
    /// "todo tipo possui representação própria e completa". Sem ela,
    /// "completa" seria julgamento subjetivo, e PL-05 não admite julgamento
    /// subjetivo como conclusão de capacidade.
    /// </summary>
    private static void ValidateTypeStructure(KnowledgeModel model, List<string> violations)
    {
        // IV-14 é invariante do C04. Uma Base que não executou o C04 tem
        // tipos sem classificação por definição, e isso não é violação: é o
        // estado correto do C03. Ler o manifesto, e não a presença de
        // `type.kind`, é o que distingue "capacidade não executada" de
        // "capacidade executada e omissa" — a segunda tem de falhar.
        if (!model.Manifest.Capabilities.Contains("C04", StringComparer.Ordinal))
            return;

        var declared = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeDeclared)
            .Select(o => o.Subject)
            .ToHashSet();

        if (declared.Count == 0)
            return;

        var kinds = Count(model, ObservationKinds.TypeKind);
        var accessibilities = Count(model, ObservationKinds.TypeAccessibility);

        // IV-14
        foreach (var type in declared.OrderBy(t => t))
        {
            if (kinds.GetValueOrDefault(type) != 1)
            {
                violations.Add(
                    $"IV-14: {type} tem {kinds.GetValueOrDefault(type)} type.kind; esperado exatamente 1");
            }

            if (accessibilities.GetValueOrDefault(type) != 1)
            {
                violations.Add(
                    $"IV-14: {type} tem {accessibilities.GetValueOrDefault(type)} "
                    + "type.accessibility; esperado exatamente 1");
            }
        }

        ValidateNesting(model, violations);
        ValidateGenericParameters(model, violations);
    }

    /// <summary>IV-15: contenção é árvore, não grafo qualquer.</summary>
    private static void ValidateNesting(KnowledgeModel model, List<string> violations)
    {
        var container = new Dictionary<KnowledgeId, KnowledgeId>();

        foreach (var observation in model.Observations
                     .Where(o => o.Kind == ObservationKinds.TypeNestedIn)
                     .OrderBy(o => o.Id))
        {
            var target = KnowledgeId.Parse(observation.Payload["containerId"]!);

            if (container.TryGetValue(observation.Subject, out var existing)
                && !existing.Equals(target))
            {
                violations.Add($"IV-15: {observation.Subject} declara mais de um contentor");
                continue;
            }

            container[observation.Subject] = target;
        }

        foreach (var start in container.Keys.OrderBy(k => k))
        {
            var seen = new HashSet<KnowledgeId> { start };

            var current = start;

            while (container.TryGetValue(current, out var next))
            {
                if (!seen.Add(next))
                {
                    violations.Add($"IV-15: ciclo de aninhamento envolvendo {start}");
                    break;
                }

                current = next;
            }
        }
    }

    /// <summary>IV-16: os ordinais formam 0..n-1, sem repetição e sem lacuna.</summary>
    private static void ValidateGenericParameters(KnowledgeModel model, List<string> violations)
    {
        var groups = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeGenericParameter)
            .GroupBy(o => o.Subject)
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var ordinals = new List<int>();

            foreach (var observation in group)
            {
                if (int.TryParse(
                        observation.Payload["ordinal"],
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var ordinal))
                {
                    ordinals.Add(ordinal);
                    continue;
                }

                violations.Add($"IV-16: ordinal não numérico em {observation.Id}");
            }

            ordinals.Sort();

            var expected = Enumerable.Range(0, ordinals.Count);

            if (!ordinals.SequenceEqual(expected))
            {
                violations.Add(
                    $"IV-16: {group.Key} tem ordinais [{string.Join(", ", ordinals)}]; "
                    + $"esperado 0..{ordinals.Count - 1}");
            }
        }
    }

    private static Dictionary<KnowledgeId, int> Count(KnowledgeModel model, string kind)
        => model.Observations
            .Where(o => o.Kind == kind)
            .GroupBy(o => o.Subject)
            .ToDictionary(g => g.Key, g => g.Count());

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

            // IV-17: um único local de declaração não sustenta conclusão
            // alguma sobre parcialidade.
            if (evidence.Kind == EvidenceKinds.TypeDeclarationSites
                && evidence.Observations.Count < 2)
            {
                violations.Add(
                    $"IV-17: Evidence {evidence.Id} agrupa menos de duas Observations");
            }

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
