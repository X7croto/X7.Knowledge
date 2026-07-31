using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// C02 — conclui sobre a posição de cada projeto no grafo de dependências.
/// Não observa nada: consome apenas Observations já produzidas.
/// </summary>
public sealed class ArchitectureProducer : IProducer
{
    private const string LayerRule = "layer-by-graph-depth";
    private const string RootRule = "root-by-absence-of-dependents";
    private const string LeafRule = "leaf-by-absence-of-references";
    private const string CycleRule = "cycle-by-strongly-connected-component";

    public string Name => nameof(ArchitectureProducer);

    public string Capability => "C02";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        var observations = context.Knowledge.Observations;

        var graph = ProjectGraph.Build(observations);

        if (graph.Nodes.Count == 0)
            return ValueTask.CompletedTask;

        var graphEvidence = context.Knowledge.AddEvidence(
            Evidence.Create(
                EvidenceKinds.ProjectGraphPosition,
                observations
                    .Where(o => o.Kind is ObservationKinds.ProjectDeclared
                                       or ObservationKinds.ProjectReferencesProject)
                    .Select(o => o.Id),
                Name,
                Capability));

        InferenceProvenance Rule(string rule) => new()
        {
            Rule = rule,
            Producer = Name,
            Capability = Capability,
            AcquisitionLevel = context.AcquisitionLevel
        };

        var depths = graph.Depths();
        var cycles = graph.StronglyConnectedComponents()
            .Where(c => c.Count > 1)
            .ToArray();

        var inCycle = cycles.SelectMany(c => c).ToHashSet();

        foreach (var project in graph.Nodes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Profundidade é exata dada a estrutura do grafo: Asserted.
            context.Knowledge.AddInference(Inference.Create(
                InferenceKinds.ProjectLayer,
                project,
                ObservationPayload.From(
                    ("depth", depths[project].ToString())),
                graphEvidence,
                Confidence.Asserted,
                Rule(LayerRule)));

            if (graph.DependentsOf(project).Count == 0)
            {
                context.Knowledge.AddInference(Inference.Create(
                    InferenceKinds.ProjectIsRoot,
                    project,
                    ObservationPayload.Empty,
                    graphEvidence,
                    Confidence.Asserted,
                    Rule(RootRule)));
            }

            if (graph.ReferencesOf(project).Count == 0)
            {
                context.Knowledge.AddInference(Inference.Create(
                    InferenceKinds.ProjectIsLeaf,
                    project,
                    ObservationPayload.Empty,
                    graphEvidence,
                    Confidence.Asserted,
                    Rule(LeafRule)));
            }
        }

        ProduceCycles(context, graph, cycles, Rule(CycleRule));

        return ValueTask.CompletedTask;
    }

    private void ProduceCycles(
        CompilationContext context,
        ProjectGraph graph,
        IReadOnlyList<IReadOnlyList<KnowledgeId>> cycles,
        InferenceProvenance provenance)
    {
        var observations = context.Knowledge.Observations;

        foreach (var cycle in cycles)
        {
            var members = cycle.ToHashSet();

            // Evidence do ciclo: exatamente as referências internas a ele.
            var edges = observations
                .Where(o => o.Kind == ObservationKinds.ProjectReferencesProject
                            && members.Contains(o.Subject)
                            && members.Contains(KnowledgeId.Parse(o.Payload["targetId"]!)))
                .Select(o => o.Id);

            var cycleEvidence = context.Knowledge.AddEvidence(
                Evidence.Create(EvidenceKinds.ProjectCyclePath, edges, Name, Capability));

            var cycleId = cycleEvidence.Id.Value;

            foreach (var member in cycle)
            {
                context.Knowledge.AddInference(Inference.Create(
                    InferenceKinds.ProjectParticipatesInCycle,
                    member,
                    ObservationPayload.From(("cycleId", cycleId)),
                    cycleEvidence,
                    Confidence.Asserted,
                    provenance));
            }
        }
    }
}
