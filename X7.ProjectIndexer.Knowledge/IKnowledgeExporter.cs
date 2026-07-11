using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Knowledge;

public interface IKnowledgeExporter
{
    void Export(ProjectIndexOld index, string outputFolder);
}