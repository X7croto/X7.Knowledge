using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Query.Builders;
using X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query;

public sealed class KnowledgeQueryBuilder
{
    public KnowledgeIndex Build(ProjectIndexOld index)
    {
        var knowledge = new KnowledgeIndex();

        knowledge.Types.AddRange(
            new TypeIndexBuilder().Build(index));

        knowledge.Methods.AddRange(
            new MethodIndexBuilder().Build(index));

        knowledge.Namespaces.AddRange(
            new NamespaceIndexBuilder().Build(index));

        knowledge.Features.AddRange(
            new FeatureIndexBuilder().Build(index));

        return knowledge;
    }
}