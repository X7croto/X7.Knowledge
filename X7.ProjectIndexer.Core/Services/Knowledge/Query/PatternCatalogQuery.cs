using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query;

public sealed class PatternCatalogQuery
    : IKnowledgeQuery<List<string>>
{
    public List<string> Execute(ProjectIndexOld index)
    {
        return index.Knowledge.Architecture.Patterns
            .OrderBy(x => x)
            .ToList();
    }
}