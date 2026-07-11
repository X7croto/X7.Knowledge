using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Knowledge;

public sealed class ClaudeExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        var md = new MarkdownWriter();

        md.H1("Claude Context");

        md.Line("Read these files in order:");

        md.Line();

        md.Bullet("architecture.md");

        md.Bullet("solution.md");

        md.Bullet("projects/");

        md.Bullet("namespaces/");

        md.Bullet("types/");

        md.Bullet("methods/");

        md.Line();

        md.H2("Architecture");

        md.Line("Use graph.json to answer dependency questions.");

        md.Line("Use semantic.json to answer symbol questions.");

        md.Line();

        md.H2("Behavior");

        md.Line("Never assume.");

        md.Line("Use the generated knowledge before reading source code.");

        File.WriteAllText(
            Path.Combine(folder, "CLAUDE.md"),
            md.ToString());
    }
}