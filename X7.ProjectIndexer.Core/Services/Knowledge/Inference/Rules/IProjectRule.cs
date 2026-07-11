using X7.ProjectIndexer.Core.Models;

public interface IProjectRule
{
    void Analyze(
        ProjectIndexOld index,
        InferenceContext context);
}