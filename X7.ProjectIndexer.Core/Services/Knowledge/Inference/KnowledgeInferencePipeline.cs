using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference;

public sealed class KnowledgeInferencePipeline
{
    private readonly KnowledgeAnalysisEngine _engine = new();

    public KnowledgeInferencePipeline()
    {
        _engine.Register(new LayerRule());

        _engine.Register(new PatternRule());

        _engine.Register(new FlowRule());

        _engine.Register(new FeatureRule());
    }

    public void Infer(ProjectIndexOld index)
    {
        _engine.Execute(index);
    }
}