using X7.Knowledge.Model.Entities;

namespace X7.Knowledge.Model;

/// <summary>
/// Modelo canônico único (PR-01). Substrato de Observations
/// mais indexação tipada derivada delas.
/// </summary>
public sealed class KnowledgeModel
{
    internal KnowledgeModel(
        Manifest manifest,
        IReadOnlyList<Observation> observations,
        EntityIndex entities)
    {
        Manifest = manifest;
        Observations = observations;
        Entities = entities;
    }

    public Manifest Manifest { get; }

    /// <summary>Ordenadas por subject, kind, id (OB-04).</summary>
    public IReadOnlyList<Observation> Observations { get; }

    /// <summary>Índice tipado. Nenhum campo aqui existe sem Observation (IV-02).</summary>
    public EntityIndex Entities { get; }
}
