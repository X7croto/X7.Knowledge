using System.Security.Cryptography;
using System.Text;

namespace X7.Knowledge.Model;

/// <summary>
/// Conhecimento derivado exclusivamente de Evidence, por regra determinística
/// e declarada (Constituição §3.1). Toda Inference aponta para sua Evidence.
/// </summary>
public sealed record Inference
{
    private Inference() { }

    public required KnowledgeId Id { get; init; }

    public required string Kind { get; init; }

    public required KnowledgeId Subject { get; init; }

    public required ObservationPayload Payload { get; init; }

    /// <summary>Evidence que sustenta. Obrigatória (IV-09).</summary>
    public required KnowledgeId Evidence { get; init; }

    public required Confidence Confidence { get; init; }

    /// <summary>Obrigatória se Observed, proibida se Asserted (IV-11).</summary>
    public Frequency? Frequency { get; init; }

    public required InferenceProvenance Provenance { get; init; }

    public static Inference Create(
        string kind,
        KnowledgeId subject,
        ObservationPayload payload,
        Evidence evidence,
        Confidence confidence,
        InferenceProvenance provenance,
        Frequency? frequency = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(provenance);

        if (!InferenceKinds.IsKnown(kind))
            throw new UnknownInferenceKindException(kind);

        if (string.IsNullOrWhiteSpace(provenance.Rule))
        {
            throw new InvalidOperationException(
                $"Inference '{kind}' sem regra declarada. Ver Constituição §3.1.");
        }

        // PR-11: a incerteza é explicitada, não sugerida.
        switch (confidence)
        {
            case Confidence.Observed when frequency is null:
                throw new InvalidOperationException(
                    $"Inference '{kind}' com Confidence Observed exige frequência declarada.");

            case Confidence.Asserted when frequency is not null:
                throw new InvalidOperationException(
                    $"Inference '{kind}' com Confidence Asserted não admite frequência: " +
                    "regra exata não tem exceções. Use Observed.");
        }

        return new Inference
        {
            Id = ComputeId(kind, subject, payload, evidence.Id),
            Kind = kind,
            Subject = subject,
            Payload = payload,
            Evidence = evidence.Id,
            Confidence = confidence,
            Frequency = frequency,
            Provenance = provenance
        };
    }

    private static KnowledgeId ComputeId(
        string kind,
        KnowledgeId subject,
        ObservationPayload payload,
        KnowledgeId evidenceId)
    {
        var material = string.Concat(
            kind, "\u0000",
            subject.Value, "\u0000",
            payload.ToCanonicalString(), "\u0000",
            evidenceId.Value);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));

        return KnowledgeId.ForInference(Convert.ToHexString(hash.AsSpan(0, 8)).ToLowerInvariant());
    }
}

public sealed class UnknownInferenceKindException(string kind)
    : InvalidOperationException($"Kind de Inference fora do catálogo: '{kind}'.")
{
    public string Kind { get; } = kind;
}
