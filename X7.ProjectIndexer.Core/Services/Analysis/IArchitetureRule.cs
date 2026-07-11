using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public interface IArchitectureRule
{
    void Analyze(ProjectIndexOld index);
}