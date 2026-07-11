using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query;

public sealed class KnowledgeQueries
{
    private readonly ProjectIndexOld _index;

    public KnowledgeQueries(ProjectIndexOld index)
    {
        _index = index;
    }

    public IReadOnlyList<FeatureModel> Features()
        => new FeatureCatalogQuery().Execute(_index);

    public IReadOnlyList<ServiceModel> Services()
        => new ServiceCatalogQuery().Execute(_index);

    public IReadOnlyList<string> Layers()
        => new LayerCatalogQuery().Execute(_index);

    public IReadOnlyList<string> Patterns()
        => new PatternCatalogQuery().Execute(_index);
}