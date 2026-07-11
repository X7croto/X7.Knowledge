using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public interface IAnalyzer
{
    void Analyze(ProjectIndexOld index);
}