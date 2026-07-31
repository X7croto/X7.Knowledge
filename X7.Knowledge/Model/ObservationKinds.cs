namespace X7.Knowledge.Model;

/// <summary>
/// Catálogo de kinds da v0 (KNOWLEDGE_MODEL §6).
/// Kind fora do catálogo é erro de compilação, nunca item ignorado (OB-02, IV-04).
/// </summary>
public static class ObservationKinds
{
    public const string SolutionDeclared = "solution.declared";
    public const string SolutionContainsProject = "solution.contains-project";
    public const string SolutionFolder = "solution.folder";
    public const string SolutionFolderContains = "solution.folder-contains";

    public const string ProjectDeclared = "project.declared";
    public const string ProjectTargetFramework = "project.target-framework";
    public const string ProjectOutputKind = "project.output-kind";
    public const string ProjectLanguageVersion = "project.language-version";
    public const string ProjectProperty = "project.property";
    public const string ProjectIsTestProject = "project.is-test-project";

    // C02
    public const string ProjectReferencesProject = "project.references-project";
    public const string ProjectPackageReference = "project.package-reference";

    // C03
    public const string NamespaceDeclared = "namespace.declared";
    public const string NamespaceContains = "namespace.contains";
    public const string TypeDeclared = "type.declared";
    public const string TypeLocation = "type.location";

    public const string AcquisitionLimitation = "acquisition.limitation";

    private static readonly HashSet<string> Catalog = new(StringComparer.Ordinal)
    {
        SolutionDeclared,
        SolutionContainsProject,
        SolutionFolder,
        SolutionFolderContains,
        ProjectDeclared,
        ProjectTargetFramework,
        ProjectOutputKind,
        ProjectLanguageVersion,
        ProjectProperty,
        ProjectIsTestProject,
        ProjectReferencesProject,
        ProjectPackageReference,
        NamespaceDeclared,
        NamespaceContains,
        TypeDeclared,
        TypeLocation,
        AcquisitionLimitation
    };

    public static bool IsKnown(string kind) => Catalog.Contains(kind);

    public static IReadOnlyCollection<string> All => Catalog;
}
