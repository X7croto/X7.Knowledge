namespace X7.Knowledge.Model;

/// <summary>
/// Catálogo de kinds de Evidence. Fechado como o de Observation:
/// kind desconhecido é erro de compilação (IV-04).
/// </summary>
public static class EvidenceKinds
{
    /// <summary>
    /// O grafo inteiro: nós (project.declared) e arestas
    /// (project.references-project). Toda conclusão sobre posição no grafo
    /// depende do grafo completo, não de um trecho dele.
    /// </summary>
    public const string ProjectGraphPosition = "project.graph-position";

    /// <summary>Referências que fecham um ciclo.</summary>
    public const string ProjectCyclePath = "project.cycle-path";

    /// <summary>
    /// C04 — as `type.location` de um mesmo tipo. Só é registrada com duas ou
    /// mais Observations (IV-17): um único local não sustenta conclusão
    /// alguma sobre parcialidade.
    /// </summary>
    public const string TypeDeclarationSites = "type.declaration-sites";

    private static readonly HashSet<string> Catalog = new(StringComparer.Ordinal)
    {
        ProjectGraphPosition,
        ProjectCyclePath,
        TypeDeclarationSites
    };

    public static bool IsKnown(string kind) => Catalog.Contains(kind);

    public static IReadOnlyCollection<string> All => Catalog;
}
