using X7.ProjectIndexer.Core.Integrity;
using X7.ProjectIndexer.Core.Models.Analysis;
using X7.ProjectIndexer.Core.Models.Graph;
using X7.ProjectIndexer.Core.Models.Knowledge;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;
using X7.ProjectIndexer.Core.Services.Graph;
using X7.ProjectIndexer.Core.Services.Knowledge.Query;
using X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;
using X7.ProjectIndexer.Core.Services.Query;

namespace X7.ProjectIndexer.Core.Models;

public sealed class ProjectIndexOld
{
    public required string RootPath { get; init; }

    public List<ProjectNode> Projects { get; } = [];

    public SymbolTable Semantic { get; } = new();

    public SemanticGraph Graph { get; } = new();

    public AnalysisResult Analysis { get; } = new();

    public IGraphQueryService? GraphQueries { get; set; }

    public SymbolQuery? Query { get; set; }

    public KnowledgeModel Knowledge { get; } = new();

    public KnowledgeQueries KnowledgeQuery { get; set; } = null!;

    public Dictionary<TypeSymbol, TypeAnalysis> TypeAnalysis { get; } = [];

    public KnowledgeIndex QueryIndex { get; set; } = new();

    public IntegrityValidationContext? Integrity { get; set; }

    public Dictionary<string, TypeNode> ParsedTypesByFullName { get; }
    = new();

    public Dictionary<string, List<TypeNode>> ParsedTypesByName { get; }
        = new();
}