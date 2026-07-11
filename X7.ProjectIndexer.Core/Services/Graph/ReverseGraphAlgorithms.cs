using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Graph;

public static partial class GraphAlgorithms
{
    public static HashSet<T> ReverseDepthFirstSearch<T>(
        T root,
        Func<T, IEnumerable<T>> parents)
        where T : class
    {
        var visited = new HashSet<T>();

        Visit(root);

        return visited;

        void Visit(T node)
        {
            if (!visited.Add(node))
                return;

            foreach (var parent in parents(node))
                Visit(parent);
        }
    }
}