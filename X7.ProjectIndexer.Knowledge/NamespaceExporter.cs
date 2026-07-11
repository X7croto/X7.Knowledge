using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Knowledge;

public sealed class NamespaceExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        var namespaces = index.Semantic.Types
            .GroupBy(t => t.Namespace)
            .OrderBy(g => g.Key);

        foreach (var group in namespaces)
        {
            var md = new MarkdownWriter();

            md.H1(group.Key);

            md.Line($"Types: {group.Count()}");
            md.Line();

            foreach (var type in group.OrderBy(x => x.Name))
            {
                md.Bullet($"{type.Kind} {type.Name}");
            }

            var name = Sanitize(group.Key) + ".md";

            File.WriteAllText(
                Path.Combine(folder, "namespaces", name),
                md.ToString());
        }
    }

    private static string Sanitize(string text)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            text = text.Replace(c, '_');

        return text.Replace('.', '_');
    }
}