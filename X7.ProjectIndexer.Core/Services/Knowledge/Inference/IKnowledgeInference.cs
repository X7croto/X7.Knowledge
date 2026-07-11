using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference;

public interface IKnowledgeInference
{
    void Infer(ProjectIndexOld index);
}