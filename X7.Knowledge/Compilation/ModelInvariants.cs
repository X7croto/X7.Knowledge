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
        ValidateMemberSurface(model, violations);
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
            foreach (var key in (string[])["baseTypeId", "interfaceId", "containerId", "typeId"])
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

    /// <summary>
    /// IV-18 a IV-21 — consistência da superfície declarada (C05).
    /// </summary>
    /// <remarks>
    /// Sem guarda de manifesto, ao contrário da IV-14, e por um motivo que
    /// vale registrar: estes são invariantes de **consistência**, não de
    /// cobertura. A IV-14 pôde exigir que todo tipo tenha classificação
    /// porque todo tipo tem uma; não existe equivalente para membro, porque
    /// tipo sem membro é legítimo e o modelo não sabe o que ficou de fora.
    /// Numa Base sem C05 eles são vacuamente verdadeiros.
    ///
    /// A consequência está declarada na ADR-039: o critério 1 do C05 não fica
    /// testável aqui e passa a depender do critério 2, a conferência de
    /// assinatura contra o compilador de referência.
    /// </remarks>
    private static void ValidateMemberSurface(KnowledgeModel model, List<string> violations)
    {
        var declared = model.Observations
            .Where(o => o.Kind == ObservationKinds.MemberDeclared)
            .ToArray();

        var kindOf = new Dictionary<KnowledgeId, string>();

        foreach (var group in declared.GroupBy(o => o.Subject).OrderBy(g => g.Key))
        {
            if (group.Count() != 1)
            {
                violations.Add(
                    $"IV-18: {group.Key} tem {group.Count()} member.declared; esperado exatamente 1");
            }

            var kind = group.First().Payload["kind"];

            if (kind is null || !MemberVocabulary.IsKnownKind(kind))
            {
                violations.Add($"IV-04: espécie de membro fora do vocabulário em {group.Key}: {kind}");
                continue;
            }

            kindOf[group.Key] = kind;
        }

        var accessibilities = Count(model, ObservationKinds.MemberAccessibility);
        var types = Count(model, ObservationKinds.MemberType);

        var containers = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeDeclaresMember)
            .GroupBy(o => KnowledgeId.Parse(o.Payload["memberId"]!))
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var (member, kind) in kindOf.OrderBy(p => p.Key))
        {
            // IV-18
            if (accessibilities.GetValueOrDefault(member) != 1)
            {
                violations.Add(
                    $"IV-18: {member} tem {accessibilities.GetValueOrDefault(member)} "
                    + "member.accessibility; esperado exatamente 1");
            }

            if (containers.GetValueOrDefault(member) != 1)
            {
                violations.Add(
                    $"IV-18: {member} é alvo de {containers.GetValueOrDefault(member)} "
                    + "type.declares-member; esperado exatamente 1");
            }

            // IV-19. Construtor não escreve tipo na declaração, inclusive o
            // estático; todas as demais espécies escrevem exatamente um.
            var esperado = kind == MemberVocabulary.Constructor ? 0 : 1;

            if (types.GetValueOrDefault(member) != esperado)
            {
                violations.Add(
                    $"IV-19: {member} tem {types.GetValueOrDefault(member)} member.type; "
                    + $"esperado {esperado} para espécie '{kind}'");
            }
        }

        // Uma contenção que aponta para membro inexistente esconde ausência:
        // o tipo pareceria declarar algo que a Base não tem.
        foreach (var member in containers.Keys.Where(m => !kindOf.ContainsKey(m)).OrderBy(m => m))
            violations.Add($"IV-18: type.declares-member referencia membro inexistente: {member}");

        ValidateMemberOrdinals(model, ObservationKinds.MemberParameter, "IV-20", violations);
        ValidateMemberOrdinals(model, ObservationKinds.MemberGenericParameter, "IV-20", violations);
        ValidateAccessors(model, kindOf, violations);
        ValidateParameterOwners(model, kindOf, violations);
        ValidateConstantValues(model, kindOf, violations);

        ValidateConstraints(
            model,
            ObservationKinds.MemberGenericConstraint,
            ObservationKinds.MemberGenericParameter,
            violations);

        ValidateConstraints(
            model,
            ObservationKinds.TypeGenericConstraint,
            ObservationKinds.TypeGenericParameter,
            violations);
    }

    /// <summary>IV-20: os ordinais formam 0..n-1, sem repetição e sem lacuna.</summary>
    private static void ValidateMemberOrdinals(
        KnowledgeModel model,
        string kind,
        string invariant,
        List<string> violations)
    {
        foreach (var group in model.Observations
                     .Where(o => o.Kind == kind)
                     .GroupBy(o => o.Subject)
                     .OrderBy(g => g.Key))
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

                violations.Add($"{invariant}: ordinal não numérico em {observation.Id}");
            }

            ordinals.Sort();

            if (!ordinals.SequenceEqual(Enumerable.Range(0, ordinals.Count)))
            {
                violations.Add(
                    $"{invariant}: {group.Key} tem ordinais [{string.Join(", ", ordinals)}] "
                    + $"em {kind}; esperado 0..{ordinals.Count - 1}");
            }
        }
    }

    /// <summary>
    /// IV-24: valor de constante só existe em campo, no máximo um. Em
    /// qualquer outra espécie significaria que o Producer confundiu a forma
    /// do membro.
    /// </summary>
    private static void ValidateConstantValues(
        KnowledgeModel model,
        IReadOnlyDictionary<KnowledgeId, string> kindOf,
        List<string> violations)
    {
        foreach (var group in model.Observations
                     .Where(o => o.Kind == ObservationKinds.MemberConstantValue)
                     .GroupBy(o => o.Subject)
                     .OrderBy(g => g.Key))
        {
            var kind = kindOf.GetValueOrDefault(group.Key);

            if (kind != MemberVocabulary.Field)
                violations.Add($"IV-24: {group.Key} tem member.constant-value e é de espécie '{kind}'");

            if (group.Count() > 1)
                violations.Add($"IV-24: {group.Key} declara {group.Count()} valores de constante");
        }
    }

    /// <summary>
    /// IV-22: parâmetro só existe em membro que admite parâmetro. Campo,
    /// evento e propriedade não admitem, e uma Observation dessas ali
    /// significaria que o Producer confundiu a forma do membro.
    /// </summary>
    private static void ValidateParameterOwners(
        KnowledgeModel model,
        IReadOnlyDictionary<KnowledgeId, string> kindOf,
        List<string> violations)
    {
        var admitem = new[]
        {
            MemberVocabulary.Method,
            MemberVocabulary.Constructor,
            MemberVocabulary.Operator,
            MemberVocabulary.Indexer
        };

        foreach (var group in model.Observations
                     .Where(o => o.Kind == ObservationKinds.MemberParameter)
                     .GroupBy(o => o.Subject)
                     .OrderBy(g => g.Key))
        {
            var kind = kindOf.GetValueOrDefault(group.Key);

            if (kind is null || !admitem.Contains(kind, StringComparer.Ordinal))
            {
                violations.Add(
                    $"IV-22: {group.Key} tem member.parameter e é de espécie '{kind}'");
            }
        }
    }

    /// <summary>
    /// IV-21: acessor só existe em propriedade, indexador ou evento, um por
    /// espécie.
    /// </summary>
    private static void ValidateAccessors(
        KnowledgeModel model,
        IReadOnlyDictionary<KnowledgeId, string> kindOf,
        List<string> violations)
    {
        foreach (var group in model.Observations
                     .Where(o => o.Kind == ObservationKinds.MemberAccessor)
                     .GroupBy(o => o.Subject)
                     .OrderBy(g => g.Key))
        {
            var kind = kindOf.GetValueOrDefault(group.Key);

            var admite = kind is MemberVocabulary.Property
                or MemberVocabulary.Indexer
                or MemberVocabulary.Event;

            if (!admite)
            {
                violations.Add(
                    $"IV-21: {group.Key} tem member.accessor e é de espécie '{kind}'");
            }

            foreach (var byKind in group.GroupBy(o => o.Payload["kind"]))
            {
                if (byKind.Key is null || !MemberVocabulary.IsKnownAccessor(byKind.Key))
                {
                    violations.Add($"IV-04: acessor fora do vocabulário em {group.Key}: {byKind.Key}");
                    continue;
                }

                if (byKind.Count() > 1)
                    violations.Add($"IV-21: {group.Key} declara '{byKind.Key}' mais de uma vez");
            }
        }
    }

    /// <summary>
    /// IV-23: toda restrição referencia um parâmetro genérico declarado no
    /// mesmo sujeito, e os ordinais de um mesmo par (sujeito, parâmetro)
    /// formam `0..n-1`. É IV-20 aplicada a outro agrupamento: lá a sequência
    /// é por membro, aqui é por parâmetro dentro dele.
    /// </summary>
    private static void ValidateConstraints(
        KnowledgeModel model,
        string constraintKind,
        string parameterKind,
        List<string> violations)
    {
        var declared = model.Observations
            .Where(o => o.Kind == parameterKind)
            .GroupBy(o => o.Subject)
            .ToDictionary(
                g => g.Key,
                g => g.Select(o => o.Payload["name"] ?? string.Empty)
                      .ToHashSet(StringComparer.Ordinal));

        var groups = model.Observations
            .Where(o => o.Kind == constraintKind)
            .GroupBy(o => (o.Subject, Parameter: o.Payload["parameter"] ?? string.Empty))
            .OrderBy(g => g.Key.Subject)
            .ThenBy(g => g.Key.Parameter, StringComparer.Ordinal);

        foreach (var group in groups)
        {
            var (subject, parameter) = group.Key;

            if (!declared.TryGetValue(subject, out var names) || !names.Contains(parameter))
            {
                violations.Add(
                    $"IV-23: {subject} restringe '{parameter}', que não é parâmetro "
                    + "genérico declarado nesse sujeito");
            }

            var ordinals = new List<int>();

            foreach (var observation in group)
            {
                var form = observation.Payload["form"];

                if (form is null || !MemberVocabulary.IsKnownConstraintForm(form))
                    violations.Add($"IV-04: forma de restrição fora do vocabulário em {observation.Id}: {form}");

                if (int.TryParse(
                        observation.Payload["ordinal"],
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var ordinal))
                {
                    ordinals.Add(ordinal);
                    continue;
                }

                violations.Add($"IV-23: ordinal não numérico em {observation.Id}");
            }

            ordinals.Sort();

            if (!ordinals.SequenceEqual(Enumerable.Range(0, ordinals.Count)))
            {
                violations.Add(
                    $"IV-23: {subject} tem ordinais [{string.Join(", ", ordinals)}] nas "
                    + $"restrições de '{parameter}'; esperado 0..{ordinals.Count - 1}");
            }
        }
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

    /// <summary>
    /// IV-08. Depois da normalização exigida por D-02 o caminho perde a
    /// barra invertida, e `C:/Users/...` passava pelos três testes originais
    /// — foi assim que o nome de um usuário chegou à Base publicada
    /// (ADR-041). O texto do invariante nunca mudou; a implementação é que
    /// estava incompleta.
    /// </summary>
    private static bool LooksAbsolute(string value)
        => value.StartsWith('/')
           || value.StartsWith("../", StringComparison.Ordinal)
           || value.Contains(":\\", StringComparison.Ordinal)
           || value.StartsWith("\\\\", StringComparison.Ordinal)
           || IsDriveRooted(value);

    private static bool IsDriveRooted(string value)
        => value.Length >= 3
           && char.IsAsciiLetter(value[0])
           && value[1] == ':'
           && value[2] is '/' or '\\';
}

public sealed class InvariantViolationException(IReadOnlyList<string> violations)
    : InvalidOperationException(
        "Invariantes do KnowledgeModel violados:\n - " + string.Join("\n - ", violations))
{
    public IReadOnlyList<string> Violations { get; } = violations;
}
