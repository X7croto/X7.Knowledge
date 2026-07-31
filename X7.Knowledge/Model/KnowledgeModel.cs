using X7.Knowledge.Model.Entities;

namespace X7.Knowledge.Model;

/// <summary>
/// Modelo canônico único (PR-01). Substrato de Observations, camada derivada
/// de Evidence e Inference, mais indexação tipada.
/// </summary>
public sealed class KnowledgeModel
{
    internal KnowledgeModel(
        Manifest manifest,
        IReadOnlyList<Observation> observations,
        IReadOnlyList<Evidence> evidence,
        IReadOnlyList<Inference> inferences,
        EntityIndex entities)
    {
        Manifest = manifest;
        Observations = observations;
        Evidence = evidence;
        Inferences = inferences;
        Entities = entities;
    }

    public Manifest Manifest { get; }

    /// <summary>Ordenadas por subject, kind, id (OB-04).</summary>
    public IReadOnlyList<Observation> Observations { get; }

    /// <summary>Ordenadas por id.</summary>
    public IReadOnlyList<Evidence> Evidence { get; }

    /// <summary>Ordenadas por subject, kind, id.</summary>
    public IReadOnlyList<Inference> Inferences { get; }

    /// <summary>Índice tipado. Nenhum campo aqui existe sem origem observada (IV-02).</summary>
    public EntityIndex Entities { get; }
}
