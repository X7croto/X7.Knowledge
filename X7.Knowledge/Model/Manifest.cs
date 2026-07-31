namespace X7.Knowledge.Model;

/// <summary>
/// Torna a compilação auditável. Não contém timestamp, máquina,
/// usuário ou caminho absoluto (D-03).
/// </summary>
public sealed record Manifest
{
    public required string ModelVersion { get; init; }

    public required string CompilerVersion { get; init; }

    /// <summary>
    /// Versão do MSBuild que produziu o modelo semântico. Presente apenas em
    /// nível S.
    /// </summary>
    /// <remarks>
    /// Em nível S a saída depende do SDK instalado: ele resolve referências e
    /// alimenta o modelo semântico. Sem registrar qual, duas Bases divergentes
    /// pareceriam comparáveis, e PR-02 ficaria sem como ser verificado.
    /// Em nível X o campo é ausente, porque nada do SDK participou.
    /// </remarks>
    public string? MsBuildVersion { get; init; }

    public required KnowledgeId SolutionId { get; init; }

    public required AcquisitionLevel AcquisitionLevel { get; init; }

    /// <summary>Capacidades executadas. Ex.: ["C01"].</summary>
    public required IReadOnlyList<string> Capabilities { get; init; }

    /// <summary>Hash canônico das entradas consideradas.</summary>
    public required string InputDigest { get; init; }

    public required int ObservationCount { get; init; }

    public required int EvidenceCount { get; init; }

    public required int InferenceCount { get; init; }
}
