using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Symbols;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public sealed class PatternRule : ITypeRule
{
    public string Name => "Pattern Inference";

    public void Analyze(TypeSymbol type, InferenceContext context)
    {
        var index = context.Symbols.Index;

        var patterns = index.Knowledge.Architecture.Patterns;

        patterns.Clear();

        DetectRepository(index, patterns);
        DetectDependencyInjection(index, patterns);
        DetectCQRS(index, patterns);
        DetectMVC(index, patterns);
    }

    private static void DetectRepository(
        ProjectIndexOld index,
        ICollection<string> patterns)
    {
        if (index.Semantic.Types.Any(t =>
            t.Name.EndsWith("Repository")))
        {
            patterns.Add("Repository");
        }
    }

    private static void DetectDependencyInjection(
        ProjectIndexOld index,
        ICollection<string> patterns)
    {
        if (index.Semantic.Types.Any(t =>
            t.Name.EndsWith("Service")))
        {
            patterns.Add("Dependency Injection");
        }
    }

    private static void DetectCQRS(
        ProjectIndexOld index,
        ICollection<string> patterns)
    {
        if (index.Semantic.Types.Any(t =>
            t.Name.EndsWith("Command")) ||

            index.Semantic.Types.Any(t =>
            t.Name.EndsWith("Query")))
        {
            patterns.Add("CQRS");
        }
    }

    private static void DetectMVC(
        ProjectIndexOld index,
        ICollection<string> patterns)
    {
        if (index.Semantic.Types.Any(t =>
            t.Name.EndsWith("Controller")))
        {
            patterns.Add("MVC");
        }
    }
}