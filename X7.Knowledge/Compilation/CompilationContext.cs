using X7.Knowledge;

internal sealed class CompilationContext
{
    public string SolutionPath { get; }

    public string OutputDirectory { get; }

    public ProjectIndex ProjectIndex { get; set; }

    public KnowledgeModel Knowledge { get; }

    public CompilationContext(
        string solutionPath,
        string outputDirectory)
    {
        SolutionPath = solutionPath;
        OutputDirectory = outputDirectory;
        Knowledge = new KnowledgeModel();
    }
}