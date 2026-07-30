namespace X7.Knowledge.Acquisition;

/// <summary>Representação bruta da solução lida do disco. Não é conhecimento ainda.</summary>
public sealed record SolutionFile
{
    public required string Name { get; init; }

    /// <summary>Raiz absoluta — usada apenas em memória, nunca serializada.</summary>
    public required string RootDirectory { get; init; }

    /// <summary>Nome do arquivo de solução, relativo à raiz.</summary>
    public required string FileName { get; init; }

    public required IReadOnlyList<SolutionFolderEntry> Folders { get; init; }

    public required IReadOnlyList<ProjectEntry> Projects { get; init; }

    /// <summary>Coisas que o leitor não conseguiu resolver (acquisition.limitation).</summary>
    public required IReadOnlyList<AcquisitionLimitation> Limitations { get; init; }
}

public sealed record SolutionFolderEntry
{
    /// <summary>Caminho lógico sem barras nas pontas. Ex.: "src", "src/Core".</summary>
    public required string LogicalPath { get; init; }

    public required string Name { get; init; }

    public string? ParentLogicalPath { get; init; }
}

public sealed record ProjectEntry
{
    public required string Name { get; init; }

    /// <summary>Caminho do .csproj relativo à raiz, com '/'.</summary>
    public required string RelativePath { get; init; }

    public string? FolderLogicalPath { get; init; }
}

public sealed record AcquisitionLimitation
{
    public required string Reason { get; init; }

    public required string AffectedScope { get; init; }

    public required string Source { get; init; }

    public string? Locator { get; init; }
}
