using System.Security.Cryptography;
using System.Text;

namespace X7.Knowledge.Model;

/// <summary>Unidade atômica de conhecimento. Nunca interpreta (OB-01).</summary>
public sealed record Observation
{
    private Observation() { }

    public required KnowledgeId Id { get; init; }

    public required string Kind { get; init; }

    public required KnowledgeId Subject { get; init; }

    public required ObservationPayload Payload { get; init; }

    public required Provenance Provenance { get; init; }

    public static Observation Create(
        string kind,
        KnowledgeId subject,
        ObservationPayload payload,
        Provenance provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(provenance);

        if (!ObservationKinds.IsKnown(kind))
            throw new UnknownObservationKindException(kind);

        return new Observation
        {
            Id = ComputeId(kind, subject, payload),
            Kind = kind,
            Subject = subject,
            Payload = payload,
            Provenance = provenance
        };
    }

    /// <summary>
    /// obs:{sha256(kind + subject + payloadCanônico)[0..16]}.
    /// Observations idênticas produzem o mesmo id e deduplicam naturalmente.
    /// </summary>
    private static KnowledgeId ComputeId(
        string kind,
        KnowledgeId subject,
        ObservationPayload payload)
    {
        var material = string.Concat(
            kind,
            "\u0000",
            subject.Value,
            "\u0000",
            payload.ToCanonicalString());

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));

        var digest = Convert.ToHexString(hash.AsSpan(0, 8)).ToLowerInvariant();

        return KnowledgeId.ForObservation(digest);
    }
}

public sealed class UnknownObservationKindException(string kind)
    : InvalidOperationException($"Kind fora do catálogo: '{kind}'. Ver KNOWLEDGE_MODEL §6.")
{
    public string Kind { get; } = kind;
}
