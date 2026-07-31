namespace X7.Knowledge.Model;

/// <summary>
/// Acumula conhecimento durante a compilação.
/// Monotônico dentro da compilação: só adiciona, nunca remove (PR-05).
/// </summary>
public sealed class KnowledgeModelBuilder
{
    private readonly Dictionary<KnowledgeId, Observation> _observations = [];
    private readonly Dictionary<KnowledgeId, Evidence> _evidence = [];
    private readonly Dictionary<KnowledgeId, Inference> _inferences = [];

    public IReadOnlyCollection<Observation> Observations => _observations.Values;

    public IReadOnlyCollection<Evidence> Evidence => _evidence.Values;

    public IReadOnlyCollection<Inference> Inferences => _inferences.Values;

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

    /// <summary>
    /// Registra Evidence. As Observations referenciadas já devem existir:
    /// Evidence que aponta para o vazio não sustenta nada.
    /// </summary>
    public Evidence AddEvidence(Evidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        foreach (var observation in evidence.Observations)
        {
            if (!_observations.ContainsKey(observation))
            {
                throw new InvalidOperationException(
                    $"Evidence {evidence.Id} referencia Observation inexistente: {observation}.");
            }
        }

        _evidence.TryAdd(evidence.Id, evidence);

        return _evidence[evidence.Id];
    }

    public Inference AddInference(Inference inference)
    {
        ArgumentNullException.ThrowIfNull(inference);

        if (!_evidence.ContainsKey(inference.Evidence))
        {
            throw new InvalidOperationException(
                $"Inference {inference.Id} referencia Evidence inexistente: {inference.Evidence}.");
        }

        _inferences.TryAdd(inference.Id, inference);

        return _inferences[inference.Id];
    }

    public KnowledgeModel Build(
        string modelVersion,
        string compilerVersion,
        AcquisitionLevel level,
        IReadOnlyList<string> capabilities,
        string inputDigest)
    {
        // OB-04: ordenação por subject, depois kind, depois id.
        var observations = _observations.Values
            .OrderBy(o => o.Subject)
            .ThenBy(o => o.Kind, StringComparer.Ordinal)
            .ThenBy(o => o.Id)
            .ToArray();

        var evidence = _evidence.Values
            .OrderBy(e => e.Id)
            .ToArray();

        var inferences = _inferences.Values
            .OrderBy(i => i.Subject)
            .ThenBy(i => i.Kind, StringComparer.Ordinal)
            .ThenBy(i => i.Id)
            .ToArray();

        var entities = EntityIndexProjector.Project(observations);

        var manifest = new Manifest
        {
            ModelVersion = modelVersion,
            CompilerVersion = compilerVersion,
            SolutionId = entities.Solution.Id,
            AcquisitionLevel = level,
            Capabilities = capabilities,
            InputDigest = inputDigest,
            ObservationCount = observations.Length,
            EvidenceCount = evidence.Length,
            InferenceCount = inferences.Length
        };

        return new KnowledgeModel(manifest, observations, evidence, inferences, entities);
    }
}
