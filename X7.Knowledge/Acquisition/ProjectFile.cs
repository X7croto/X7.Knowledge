namespace X7.Knowledge.Acquisition;

/// <summary>Propriedades lidas de um .csproj. Leitura sintática pura (nível X).</summary>
public sealed record ProjectFile
{
    public required string RelativePath { get; init; }

    public required IReadOnlyList<string> TargetFrameworks { get; init; }

    public string? OutputKind { get; init; }

    public string? LanguageVersion { get; init; }

    /// <summary>Propriedades escalares declaradas diretamente no arquivo.</summary>
    public required IReadOnlyDictionary<string, string> Properties { get; init; }

    public bool IsTestProject { get; init; }

    public string? TestEvidence { get; init; }

    public required IReadOnlyList<AcquisitionLimitation> Limitations { get; init; }
}
