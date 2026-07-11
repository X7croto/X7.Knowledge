namespace X7.ProjectIndexer.Core.Services.Indexing;

using X7.ProjectIndexer.Core.Contracts;
using X7.ProjectIndexer.Core.Integrity;
using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Analysis;
using X7.ProjectIndexer.Core.Services.Binding;
using X7.ProjectIndexer.Core.Services.Graph;
using X7.ProjectIndexer.Core.Services.Knowledge;
using X7.ProjectIndexer.Core.Services.Knowledge.Query;
using X7.ProjectIndexer.Core.Services.Parsing;
using X7.ProjectIndexer.Core.Services.Query;
using X7.ProjectIndexer.Core.Services.Resolution;
using X7.ProjectIndexer.Core.Services.Scanning;
using X7.ProjectIndexer.Core.Services.Semantic;

public sealed class ProjectIndexer : IProjectIndexer
{
    private readonly FileScanner _scanner = new();

    private readonly IParser _parser = new RoslynParser();

    private readonly Binder _binder = new();

    private readonly SemanticBuilder _semanticBuilder = new();

    private readonly RelationshipBuilder _relationships = new();

    private readonly GraphBuilder _graphBuilder = new();

    private readonly AnalysisPipeline _analysis = new();

    public ProjectIndexOld Index(string root)
    {
        var index = _scanner.Scan(root);

        Console.WriteLine($"FILES: {index.Projects.Sum(p => p.Files.Count)}");

        //--------------------------------------------
        // Parsing
        //--------------------------------------------

        foreach (var project in index.Projects)
            foreach (var file in project.Files)
                _parser.Parse(file);

        //--------------------------------------------
        // Binding
        //--------------------------------------------

        Console.WriteLine("Binding...");

        _binder.Bind(index);

        Console.WriteLine("Binding OK");

        //--------------------------------------------
        // Semantic
        //--------------------------------------------

        Console.WriteLine("Semantic...");

        _semanticBuilder.Build(index);

        new SemanticLinker().Link(index.Semantic);

        Console.WriteLine("Semantic OK");

        PrintSemanticStatistics(index);

        //--------------------------------------------
        // Relationships
        //--------------------------------------------

        Console.WriteLine("Relationships...");

        _relationships.Build(index);

        Console.WriteLine("Relationships OK");

        //--------------------------------------------
        // Semantic Index
        //--------------------------------------------

        Console.WriteLine("SemanticIndex...");

        new SemanticIndexBuilder().Build(index.Semantic);

        Console.WriteLine("SemanticIndex OK");

        //--------------------------------------------
        // Graph
        //--------------------------------------------

        Console.WriteLine("Graph...");

        _graphBuilder.Build(index);

        Console.WriteLine("Graph OK");

        //--------------------------------------------
        // Knowledge
        //--------------------------------------------

        Console.WriteLine("Knowledge...");

        new KnowledgeBuilder().Build(index);

        Console.WriteLine("Knowledge OK");

        //--------------------------------------------
        // Query Index
        //--------------------------------------------

        Console.WriteLine("KnowledgeQuery...");

        index.QueryIndex =
            new KnowledgeQueryBuilder().Build(index);

        Console.WriteLine("KnowledgeQuery OK");

        //--------------------------------------------
        // Queries
        //--------------------------------------------

        Console.WriteLine("Queries...");

        index.KnowledgeQuery = new KnowledgeQueries(index);

        index.GraphQueries = new GraphQueryService(index);

        index.Query = new SymbolQuery(index.Semantic);

        Console.WriteLine("Queries OK");

        //--------------------------------------------
        // Integrity
        //--------------------------------------------

        Console.WriteLine("Integrity...");

        var validation =
            new IntegrityValidator().Validate(index);

        foreach (var group in validation.Issues.GroupBy(x => x.Code))
        {
            Console.WriteLine($"{group.Key}: {group.Count()}");
        }

        index.Integrity = validation;

        Console.WriteLine($"Integrity OK ({validation.Issues.Count} issues)");

        //--------------------------------------------
        // Analysis
        //--------------------------------------------

        Console.WriteLine("Analysis...");

        _analysis.Analyze(index);

        Console.WriteLine("Analysis OK");

        return index;
    }

    private static void PrintSemanticStatistics(ProjectIndexOld index)
    {
        Console.WriteLine();

        Console.WriteLine("===== SEMANTIC =====");

        Console.WriteLine($"Projects      : {index.Semantic.Projects.Count}");
        Console.WriteLine($"Types         : {index.Semantic.Types.Count}");
        Console.WriteLine($"Methods       : {index.Semantic.Methods.Count}");
        Console.WriteLine($"Properties    : {index.Semantic.Properties.Count}");
        Console.WriteLine($"Fields        : {index.Semantic.Fields.Count}");
        Console.WriteLine($"Parameters    : {index.Semantic.Parameters.Count}");
        Console.WriteLine($"Locals        : {index.Semantic.LocalVariables.Count}");

        Console.WriteLine("====================");

        Console.WriteLine();
    }
}