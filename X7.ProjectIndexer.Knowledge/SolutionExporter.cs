using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Knowledge;

public sealed class SolutionExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        var md = new MarkdownWriter();

        md.H1("Solution");

        md.Line($"Projects: {index.Semantic.Projects.Count}");

        md.Line($"Types: {index.Semantic.Types.Count}");

        md.Line($"Methods: {index.Semantic.Methods.Count}");

        md.Line($"Properties: {index.Semantic.Properties.Count}");

        md.Line($"Fields: {index.Semantic.Fields.Count}");

        md.Line($"Dependencies: {index.Semantic.Dependencies.Count}");

        File.WriteAllText(
            Path.Combine(folder, "solution.md"),
            md.ToString());
    }
}