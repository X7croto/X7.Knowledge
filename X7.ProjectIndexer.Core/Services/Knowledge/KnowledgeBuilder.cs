using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference;
using X7.ProjectIndexer.Core.Services.Knowledge.Query;

namespace X7.ProjectIndexer.Core.Services.Knowledge;

public sealed class KnowledgeBuilder
{
    public void Build(ProjectIndexOld index)
    {
        new ModuleCompiler().Build(index);

        new EntrypointCompiler().Build(index);

        new HotspotCompiler().Build(index);

        new ServiceCompiler().Build(index);

        new FlowCompiler().Build(index);

        new BusinessRuleCompiler().Build(index);

        new KnowledgeInferencePipeline().Infer(index);

        new KnowledgeQueryBuilder().Build(index);
    }
}