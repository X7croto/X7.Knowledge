using System.Security.Cryptography;
using System.Text;

namespace X7.Knowledge.Model;

/// <summary>
/// Agrupamento nomeado e consistente de Observations que sustenta uma
/// conclusão (Constituição §3.1).
/// </summary>
/// <remarks>
/// Evidence não carrega Source nem Locator. Sua origem física é estrutural:
/// ela aponta para Observations que já declaram, cada uma, sua proveniência
/// completa. Inventar um Source sintético seria fabricar dado — o oposto do
/// que PR-04 pede. Ficam apenas Producer e Capability: quem montou o
/// agrupamento e em que capacidade.
/// </remarks>
public sealed record Evidence
{
    private Evidence() { }

    public required KnowledgeId Id { get; init; }

    public required string Kind { get; init; }

    /// <summary>Observations que sustentam. Ordenadas; nunca vazio (IV-10).</summary>
    public required IReadOnlyList<KnowledgeId> Observations { get; init; }

    public required string Producer { get; init; }

    public required string Capability { get; init; }

    public static Evidence Create(
        string kind,
        IEnumerable<KnowledgeId> observations,
        string producer,
        string capability)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(producer);
        ArgumentException.ThrowIfNullOrWhiteSpace(capability);
        ArgumentNullException.ThrowIfNull(observations);

        if (!EvidenceKinds.IsKnown(kind))
            throw new UnknownEvidenceKindException(kind);

        var ordered = observations
            .Distinct()
            .OrderBy(o => o)
            .ToArray();

        if (ordered.Length == 0)
        {
            throw new InvalidOperationException(
                $"Evidence '{kind}' sem Observations. Conclusão sem sustentação é inválida.");
        }

        return new Evidence
        {
            Id = ComputeId(kind, ordered),
            Kind = kind,
            Observations = ordered,
            Producer = producer,
            Capability = capability
        };
    }

    private static KnowledgeId ComputeId(string kind, IReadOnlyList<KnowledgeId> observations)
    {
        var material = new StringBuilder(kind);

        foreach (var observation in observations)
            material.Append('\u0000').Append(observation.Value);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material.ToString()));

        return KnowledgeId.ForEvidence(Convert.ToHexString(hash.AsSpan(0, 8)).ToLowerInvariant());
    }
}

public sealed class UnknownEvidenceKindException(string kind)
    : InvalidOperationException($"Kind de Evidence fora do catálogo: '{kind}'.")
{
    public string Kind { get; } = kind;
}
