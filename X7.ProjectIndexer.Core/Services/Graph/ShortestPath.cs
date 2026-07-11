namespace X7.ProjectIndexer.Core.Services.Graph;

public static partial class GraphAlgorithms
{
    public static List<T> ShortestPath<T>(
        T start,
        T end,
        Func<T, IEnumerable<T>> neighbours)
        where T : class
    {
        var queue = new Queue<T>();

        var previous = new Dictionary<T, T>();

        queue.Enqueue(start);

        previous[start] = start;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (ReferenceEquals(current, end))
                break;

            foreach (var next in neighbours(current))
            {
                if (previous.ContainsKey(next))
                    continue;

                previous[next] = current;

                queue.Enqueue(next);
            }
        }

        if (!previous.ContainsKey(end))
            return [];

        var path = new List<T>();

        var node = end;

        while (!ReferenceEquals(node, start))
        {
            path.Add(node);

            node = previous[node];
        }

        path.Add(start);

        path.Reverse();

        return path;
    }
}