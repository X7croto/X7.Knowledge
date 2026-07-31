using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation;

/// <summary>
/// Grafo de dependência entre projetos, derivado das Observations
/// `project.references-project`. Estrutura de trabalho — não é conhecimento
/// e não aparece na saída. O conhecimento são as Inferences que ela sustenta.
/// </summary>
internal sealed class ProjectGraph
{
    private readonly IReadOnlyList<KnowledgeId> _nodes;
    private readonly IReadOnlyDictionary<KnowledgeId, IReadOnlyList<KnowledgeId>> _edges;

    private ProjectGraph(
        IReadOnlyList<KnowledgeId> nodes,
        IReadOnlyDictionary<KnowledgeId, IReadOnlyList<KnowledgeId>> edges)
    {
        _nodes = nodes;
        _edges = edges;
    }

    public IReadOnlyList<KnowledgeId> Nodes => _nodes;

    public IReadOnlyList<KnowledgeId> ReferencesOf(KnowledgeId project)
        => _edges.TryGetValue(project, out var targets) ? targets : [];

    public IReadOnlyList<KnowledgeId> DependentsOf(KnowledgeId project)
        => _nodes
            .Where(n => ReferencesOf(n).Contains(project))
            .OrderBy(n => n)
            .ToArray();

    public static ProjectGraph Build(IReadOnlyCollection<Observation> observations)
    {
        var nodes = observations
            .Where(o => o.Kind == ObservationKinds.ProjectDeclared)
            .Select(o => o.Subject)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        var known = nodes.ToHashSet();

        var edges = observations
            .Where(o => o.Kind == ObservationKinds.ProjectReferencesProject)
            .Select(o => (From: o.Subject, To: KnowledgeId.Parse(o.Payload["targetId"]!)))
            // Referência para fora da solução não é aresta do grafo;
            // já foi declarada como limitação pelo Producer.
            .Where(e => known.Contains(e.To))
            .GroupBy(e => e.From)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<KnowledgeId>)g
                    .Select(e => e.To)
                    .Distinct()
                    .OrderBy(id => id)
                    .ToArray());

        return new ProjectGraph(nodes, edges);
    }

    /// <summary>
    /// Componentes fortemente conexos (Tarjan). Componente com mais de um
    /// membro é ciclo. Ordem de saída estável: entrada já vem ordenada.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<KnowledgeId>> StronglyConnectedComponents()
    {
        var index = new Dictionary<KnowledgeId, int>();
        var lowLink = new Dictionary<KnowledgeId, int>();
        var onStack = new HashSet<KnowledgeId>();
        var stack = new Stack<KnowledgeId>();
        var components = new List<IReadOnlyList<KnowledgeId>>();
        var next = 0;

        void Visit(KnowledgeId node)
        {
            index[node] = next;
            lowLink[node] = next;
            next++;

            stack.Push(node);
            onStack.Add(node);

            foreach (var target in ReferencesOf(node))
            {
                if (!index.ContainsKey(target))
                {
                    Visit(target);
                    lowLink[node] = Math.Min(lowLink[node], lowLink[target]);
                }
                else if (onStack.Contains(target))
                {
                    lowLink[node] = Math.Min(lowLink[node], index[target]);
                }
            }

            if (lowLink[node] != index[node])
                return;

            var component = new List<KnowledgeId>();

            KnowledgeId member;

            do
            {
                member = stack.Pop();
                onStack.Remove(member);
                component.Add(member);
            }
            while (!member.Equals(node));

            component.Sort();
            components.Add(component);
        }

        foreach (var node in _nodes)
        {
            if (!index.ContainsKey(node))
                Visit(node);
        }

        return components
            .OrderBy(c => c[0])
            .ToArray();
    }

    /// <summary>
    /// Profundidade de cada projeto: 0 para quem não referencia nenhum
    /// projeto da solução, e um a mais que a maior profundidade entre os
    /// referenciados nos demais casos.
    /// </summary>
    /// <remarks>
    /// Calculada sobre a condensação em componentes fortemente conexos, para
    /// que a presença de ciclo não torne a profundidade indefinida. Membros
    /// de um mesmo ciclo compartilham profundidade — é o que a posição no
    /// grafo de fato determina.
    /// </remarks>
    public IReadOnlyDictionary<KnowledgeId, int> Depths()
    {
        var components = StronglyConnectedComponents();

        var componentOf = new Dictionary<KnowledgeId, int>();

        for (var i = 0; i < components.Count; i++)
        {
            foreach (var member in components[i])
                componentOf[member] = i;
        }

        var componentDepth = new int?[components.Count];

        int DepthOf(int component)
        {
            if (componentDepth[component] is { } cached)
                return cached;

            // Marca antes de descer: a condensação é acíclica, mas o guarda
            // protege contra entrada corrompida.
            componentDepth[component] = 0;

            var depth = 0;

            foreach (var member in components[component])
            {
                foreach (var target in ReferencesOf(member))
                {
                    var targetComponent = componentOf[target];

                    if (targetComponent == component)
                        continue;

                    depth = Math.Max(depth, DepthOf(targetComponent) + 1);
                }
            }

            componentDepth[component] = depth;

            return depth;
        }

        var result = new Dictionary<KnowledgeId, int>();

        foreach (var node in _nodes)
            result[node] = DepthOf(componentOf[node]);

        return result;
    }
}
