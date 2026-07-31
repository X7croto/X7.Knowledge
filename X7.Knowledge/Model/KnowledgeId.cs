namespace X7.Knowledge.Model;

/// <summary>
/// Identidade estável, legível e derivada de posição lógica ou de conteúdo.
/// Nunca de GUID, contador ou endereço (D-05).
/// </summary>
public readonly record struct KnowledgeId : IComparable<KnowledgeId>
{
    private KnowledgeId(string value) => Value = value;

    public string Value { get; }

    public static KnowledgeId ForSolution(string name)
        => new($"sln:{Require(name)}");

    public static KnowledgeId ForProject(string relativePath)
        => new($"proj:{Require(relativePath)}");

    public static KnowledgeId ForSolutionFolder(string logicalPath)
        => new($"slnfolder:{Require(logicalPath)}");

    public static KnowledgeId ForDirectory(string relativePath)
        => new($"dir:{Require(relativePath)}");

    internal static KnowledgeId ForObservation(string digest)
        => new($"obs:{Require(digest)}");

    public static KnowledgeId ForNamespace(string fullName)
        => new($"ns:{Require(fullName)}");

    /// <summary>
    /// Tipo é identificado pelo nome qualificado de metadados mais o projeto.
    /// O mesmo nome em assemblies diferentes são tipos diferentes em .NET;
    /// omitir o projeto fundiria dois tipos distintos numa identidade só.
    /// </summary>
    public static KnowledgeId ForType(string metadataName, string projectName)
        => new($"type:{Require(metadataName)}@{Require(projectName)}");

    internal static KnowledgeId ForEvidence(string digest)
        => new($"ev:{Require(digest)}");

    internal static KnowledgeId ForInference(string digest)
        => new($"inf:{Require(digest)}");

    /// <summary>Reidratação a partir da forma serializada.</summary>
    public static KnowledgeId Parse(string value) => new(Require(value));

    private static string Require(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value;
    }

    // D-01: comparação e ordenação ordinais, invariantes de cultura.
    public int CompareTo(KnowledgeId other)
        => string.CompareOrdinal(Value, other.Value);

    public override string ToString() => Value ?? string.Empty;
}
