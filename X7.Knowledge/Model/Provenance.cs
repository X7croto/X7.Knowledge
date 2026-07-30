namespace X7.Knowledge.Model;

/// <summary>
/// Obrigatória em toda Observation. Sem ela, a compilação falha (PR-04, IV-01).
/// </summary>
public sealed record Provenance
{
    /// <summary>Origem física, relativa à raiz da solução (D-02).</summary>
    public required string Source { get; init; }

    /// <summary>Localização dentro da origem, quando aplicável.</summary>
    public string? Locator { get; init; }

    /// <summary>Nome do Producer responsável.</summary>
    public required string Producer { get; init; }

    /// <summary>Capacidade que produziu o item. Ex.: C01.</summary>
    public required string Capability { get; init; }

    /// <summary>Nível deste item específico, não apenas o global.</summary>
    public required AcquisitionLevel AcquisitionLevel { get; init; }
}
