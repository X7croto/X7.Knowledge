using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Analysis;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class LayerAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        Console.WriteLine("Layer: reset");

        foreach (var type in index.Semantic.Types)
            type.Layer = -1;

        Console.WriteLine("Layer: enqueue roots");

        var queue = new Queue<TypeSymbol>();

        foreach (var type in index.Semantic.Types)
        {
            if (type.FanOut == 0)
            {
                type.Layer = 0;
                queue.Enqueue(type);
            }
        }

        Console.WriteLine($"Roots: {queue.Count}");

        Console.WriteLine("Layer: bfs");

        int processed = 0;

        while (queue.Count > 0)
        {
            processed++;

            if (processed % 100 == 0)
                Console.WriteLine($"Processed {processed}");
            
            var current = queue.Dequeue();

            if (!index.Semantic.DependenciesByTarget.TryGetValue(current, out var incoming))
                continue;

            foreach (var dependency in incoming)
            {
                var source = dependency.Source;

                if (processed > 100000)
                    throw new Exception("Infinite loop detected.");

                if (source.Layer >= current.Layer + 1)
                    continue;

                source.Layer = current.Layer + 1;

                queue.Enqueue(source);
            }
        }

        foreach (var group in index.Semantic.Types.GroupBy(x => x.Layer).OrderBy(x => x.Key))
        {
            var layer = new Layer
            {
                Level = group.Key
            };

            layer.Types.AddRange(group);

            index.Analysis.Layers.Add(layer);
        }

        Console.WriteLine("Layer: grouping");
    }
}