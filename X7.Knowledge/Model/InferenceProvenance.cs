namespace X7.Knowledge.Model;

/// <summary>
/// Proveniência de uma Inference.
/// </summary>
/// <remarks>
/// Difere de <see cref="Provenance"/> por um motivo estrutural: uma Observation
/// vem de um arquivo, uma Inference vem de uma regra. A §3.1 exige que a regra
/// seja "determinística e declarada" — <see cref="Rule"/> é essa declaração.
/// A origem física continua alcançável, via Evidence, até cada Observation.
/// </remarks>
public sealed record InferenceProvenance
{
    /// <summary>Identificador estável da regra aplicada. Ex.: `layer-by-graph-depth`.</summary>
    public required string Rule { get; init; }

    public required string Producer { get; init; }

    public required string Capability { get; init; }

    public required AcquisitionLevel AcquisitionLevel { get; init; }
}
