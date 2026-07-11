using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Graph;

public static partial class GraphAlgorithms
{
    public static HashSet<T> DepthFirstSearch<T>(
        T root,
        Func<T, IEnumerable<T>> neighbours)
        where T : class
    {
        var visited = new HashSet<T>();

        Visit(root);

        return visited;

        void Visit(T node)
        {
            if (!visited.Add(node))
                return;

            foreach (var next in neighbours(node))
                Visit(next);
        }
    }

    public static HashSet<T> BreadthFirstSearch<T>(
        T root,
        Func<T, IEnumerable<T>> neighbours)
        where T : class
    {
        var visited = new HashSet<T>();

        var queue = new Queue<T>();

        visited.Add(root);

        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var next in neighbours(current))
            {
                if (!visited.Add(next))
                    continue;

                queue.Enqueue(next);
            }
        }

        return visited;
    }
}