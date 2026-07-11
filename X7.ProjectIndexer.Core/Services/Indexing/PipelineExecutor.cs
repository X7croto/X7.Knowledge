namespace X7.ProjectIndexer.Core.Services.Indexing;

using X7.ProjectIndexer.Core.Models;

public sealed class PipelineExecutor
{
    private readonly IReadOnlyList<IPipelineStep> _steps;

    public PipelineExecutor(IEnumerable<IPipelineStep> steps)
    {
        _steps = steps
            .OrderBy(s => s.Stage)
            .ToList();
    }

    public void Execute(PipelineContext context)
    {
        foreach (var step in _steps)
        {
            step.Execute(context);
        }
    }
}