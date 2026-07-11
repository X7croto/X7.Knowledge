using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query;

public sealed class FeatureCatalogQuery
    : IKnowledgeQuery<List<FeatureModel>>
{
    public List<FeatureModel> Execute(ProjectIndexOld index)
    {
        return index.Knowledge.Architecture.Features
            .OrderBy(f => f.Name)
            .ToList();
    }
}