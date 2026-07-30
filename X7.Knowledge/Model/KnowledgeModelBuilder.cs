namespace X7.Knowledge.Model;

/// <summary>
/// Acumula Observations durante a compilação.
/// Monotônico dentro da compilação: só adiciona, nunca remove (PR-05).
/// </summary>
public sealed class KnowledgeModelBuilder
{
    private readonly Dictionary<KnowledgeId, Observation> _observations = [];

    public IReadOnlyCollection<Observation> Observations => _observations.Values;

    public Observation Add(Observation observation)
    {
        ArgumentNullException.ThrowIfNull(observation);

        if (_observations.TryGetValue(observation.Id, out var existing))
        {
            // IV-05: mesmo id com payload diferente é corrupção de identidade.
            if (!existing.Payload.ToCanonicalString()
                    .Equals(observation.Payload.ToCanonicalString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Colisão de identidade em {observation.Id}: payloads divergentes.");
            }

            return existing;
        }

        _observations.Add(observation.Id, observation);

        return observation;
    }

    public Observation Add(
        string kind,
        KnowledgeId subject,
        ObservationPayload payload,
        Provenance provenance)
        => Add(Observation.Create(kind, subject, payload, provenance));

    public KnowledgeModel Build(
        string modelVersion,
        string compilerVersion,
        AcquisitionLevel level,
        IReadOnlyList<string> capabilities,
        string inputDigest)
    {
        // OB-04: ordenação por subject, depois kind, depois id.
        var ordered = _observations.Values
            .OrderBy(o => o.Subject)
            .ThenBy(o => o.Kind, StringComparer.Ordinal)
            .ThenBy(o => o.Id)
            .ToArray();

        var entities = EntityIndexProjector.Project(ordered);

        var manifest = new Manifest
        {
            ModelVersion = modelVersion,
            CompilerVersion = compilerVersion,
            SolutionId = entities.Solution.Id,
            AcquisitionLevel = level,
            Capabilities = capabilities,
            InputDigest = inputDigest,
            ObservationCount = ordered.Length
        };

        return new KnowledgeModel(manifest, ordered, entities);
    }
}
