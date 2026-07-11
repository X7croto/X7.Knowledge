using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;

public sealed class KnowledgeAnalysisEngine
{
    private readonly List<IProjectRule> _projectRules = [];

    private readonly List<ITypeRule> _typeRules = [];

    private readonly List<IMethodRule> _methodRules = [];

    private readonly List<IPropertyRule> _propertyRules = [];

    private readonly List<IFieldRule> _fieldRules = [];

    private readonly List<IProjectRule> _relationshipRules =
    [
        new DependencyRule(),
        new ArchitectureStyleRule(),
        new FlowRule()
    ];
    public void Register(object rule)
    {
        if (rule is IProjectRule project)
            _projectRules.Add(project);

        if (rule is ITypeRule type)
            _typeRules.Add(type);

        if (rule is IMethodRule method)
            _methodRules.Add(method);

        if (rule is IPropertyRule property)
            _propertyRules.Add(property);

        if (rule is IFieldRule field)
            _fieldRules.Add(field);
    }

    public void Execute(ProjectIndexOld index)
    {
        var context = new InferenceContext
        {
            Symbols = new SymbolContext
            {
                Index = index,
                Semantic = index.Semantic,
                Architecture = index.Knowledge.Architecture
            }
        };

        foreach (var rule in _projectRules)
            rule.Analyze(index, context);

        foreach (var type in index.Semantic.Types)
            foreach (var rule in _typeRules)
                rule.Analyze(type, context);

        foreach (var method in index.Semantic.Methods)
            foreach (var rule in _methodRules)
                rule.Analyze(method, context);

        foreach (var property in index.Semantic.Properties)
            foreach (var rule in _propertyRules)
                rule.Visit(property, context);

        foreach (var field in index.Semantic.Fields)
            foreach (var rule in _fieldRules)
                rule.Visit(field, context);

        foreach (var rule in _relationshipRules)
            rule.Analyze(index, context);
    }
}