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

    private static readonly HashSet<string> Catalog = new(StringComparer.Ordinal)
    {
        ProjectLayer,
        ProjectIsRoot,
        ProjectIsLeaf,
        ProjectParticipatesInCycle
    };

    public static bool IsKnown(string kind) => Catalog.Contains(kind);

    public static IReadOnlyCollection<string> All => Catalog;
}
