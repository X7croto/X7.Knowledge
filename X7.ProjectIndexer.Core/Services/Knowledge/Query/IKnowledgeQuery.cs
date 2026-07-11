using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query;

public interface IKnowledgeQuery<T>
{
    T Execute(ProjectIndexOld index);
}