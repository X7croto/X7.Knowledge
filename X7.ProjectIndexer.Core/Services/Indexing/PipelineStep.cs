namespace X7.ProjectIndexer.Core.Services.Indexing;

using X7.ProjectIndexer.Core.Models;

public interface IPipelineStep
{
    PipelineStage Stage { get; }

    void Execute(PipelineContext context);
}