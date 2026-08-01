namespace X7.Knowledge.Model;

/// <summary>
/// Catálogo de kinds de Inference. Toda Inference deste catálogo declara
/// obrigatoriamente Evidence, Confidence e regra.
/// </summary>
public static class InferenceKinds
{
    // Reservados para C02. Ainda não produzidos por nenhum Producer.
    public const string ProjectLayer = "project.layer";
    public const string ProjectIsRoot = "project.is-root";
    public const string ProjectIsLeaf = "project.is-leaf";
    public const string ProjectParticipatesInCycle = "project.participates-in-cycle";

    /// <summary>
    /// C04 — tipo declarado em mais de um arquivo é necessariamente `partial`.
    /// A recíproca é falsa: `partial` com um único local existe e não é
    /// detectado. Por isso a compilação declara a limitação correspondente.
    /// </summary>
    public const string TypeIsPartial = "type.is-partial";

    private static readonly HashSet<string> Catalog = new(StringComparer.Ordinal)
    {
        ProjectLayer,
        ProjectIsRoot,
        ProjectIsLeaf,
        ProjectParticipatesInCycle,
        TypeIsPartial
    };

    public static bool IsKnown(string kind) => Catalog.Contains(kind);

    public static IReadOnlyCollection<string> All => Catalog;
}
