using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Knowledge;

public sealed class KnowledgeGenerationReport
{
    public void Export(ProjectIndexOld index, string folder)
    {
        var md = new MarkdownWriter();

        md.H1("Knowledge Generation Report");

        md.Bullet($"Projects: {index.Semantic.Projects.Count}");
        md.Bullet($"Types: {index.Semantic.Types.Count}");
        md.Bullet($"Methods: {index.Semantic.Methods.Count}");
        md.Bullet($"Dependencies: {index.Semantic.Dependencies.Count}");
        md.Bullet($"Generated: {DateTime.Now}");

        File.WriteAllText(
            Path.Combine(folder, "reports", "generation.md"),
            md.ToString());
    }
}