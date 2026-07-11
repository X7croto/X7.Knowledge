using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Knowledge;

public sealed class ProjectExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        foreach (var project in index.Semantic.Projects)
        {
            var md = new MarkdownWriter();

            md.H1(project.Name);

            md.Line($"Types: {project.Types.Count}");

            md.Line();

            md.H2("Namespaces");

            foreach (var ns in project.Types
                .Select(x => x.Namespace)
                .Distinct()
                .OrderBy(x => x))
            {
                md.Bullet(ns);
            }

            md.Line();

            md.H2("Types");

            foreach (var type in project.Types.OrderBy(x => x.Name))
            {
                md.Bullet(type.Name);
            }

            File.WriteAllText(
                Path.Combine(folder, "projects", $"{project.Name}.md"),
                md.ToString());
        }
    }
}