using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Knowledge;

public sealed class TypeExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        foreach (var type in index.Semantic.Types.OrderBy(x => x.Name))
        {
            var md = new MarkdownWriter();

            md.H1(type.Name);

            md.Line($"Namespace: {type.Namespace}");
            md.Line($"Kind: {type.Kind}");
            md.Line($"Accessibility: {type.Accessibility}");
            md.Line();

            md.H2("Metrics");

            md.Line($"FanIn: {type.FanIn}");
            md.Line($"FanOut: {type.FanOut}");
            md.Line($"Layer: {type.Layer}");
            md.Line($"Instability: {type.Instability:F2}");
            md.Line();

            md.H2("Methods");

            foreach (var method in type.Methods.OrderBy(x => x.Name))
                md.Bullet(method.Name);

            md.Line();

            md.H2("Properties");

            foreach (var property in type.Properties.OrderBy(x => x.Name))
                md.Bullet(property.Name);

            md.Line();

            md.H2("Fields");

            foreach (var field in type.Fields.OrderBy(x => x.Name))
                md.Bullet(field.Name);

            md.Line();

            md.H2("Depends On");

            if (index.Semantic.DependenciesBySource.TryGetValue(type, out var deps))
            {
                foreach (var dependency in deps
                             .Select(x => x.Target)
                             .Distinct()
                             .OrderBy(x => x.Name))
                {
                    md.Bullet(dependency.Name);
                }
            }

            md.Line();

            md.H2("Referenced By");

            if (index.Semantic.DependenciesByTarget.TryGetValue(type, out var incoming))
            {
                foreach (var dependency in incoming
                             .Select(x => x.Source)
                             .Distinct()
                             .OrderBy(x => x.Name))
                {
                    md.Bullet(dependency.Name);
                }
            }

            var file = Path.Combine(
                folder,
                "types",
                $"{type.Name}.md");

            File.WriteAllText(file, md.ToString());
        }
    }
}