namespace X7.ProjectIndexer.Core.Services.Indexing;

public enum PipelineStage
{
    Scan,
    Parse,
    Semantic,
    Relationship,
    SemanticIndex,
    Graph,
    Knowledge,
    Query,
    IntegrityPre,
    IntegrityPost,
    Analysis
}