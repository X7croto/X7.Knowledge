namespace X7.Knowledge.Model;

/// <summary>
/// Torna a compilação auditável. Não contém timestamp, máquina,
/// usuário ou caminho absoluto (D-03).
/// </summary>
public sealed record Manifest
{
    public required string ModelVersion { get; init; }

    public required string CompilerVersion { get; init; }

    public required KnowledgeId SolutionId { get; init; }

    public required AcquisitionLevel AcquisitionLevel { get; init; }

    /// <summary>Capacidades executadas. Ex.: ["C01"].</summary>
    public required IReadOnlyList<string> Capabilities { get; init; }

    /// <summary>Hash canônico das entradas consideradas.</summary>
    public required string InputDigest { get; init; }

    public required int ObservationCount { get; init; }
}
