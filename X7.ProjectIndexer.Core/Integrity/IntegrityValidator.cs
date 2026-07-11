namespace X7.ProjectIndexer.Core.Integrity;

using X7.ProjectIndexer.Core.Integrity.Rules;
using X7.ProjectIndexer.Core.Models;

public sealed class IntegrityValidator
{
    private readonly IReadOnlyList<IntegrityValidationRule> _rules;

    public IntegrityValidator()
    {
        _rules =
        [
            new BrokenReferenceRule(),
            new DuplicateMemberReferenceRule(),
            new MethodScopeConsistencyRule(),
        ];
    }

    public IntegrityValidationContext Validate(ProjectIndexOld index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var context = new IntegrityValidationContext();

        foreach (var rule in _rules)
        {
            ExecuteRule(rule, index, context);
        }

        return context;
    }

    private static void ExecuteRule(
        IntegrityValidationRule rule,
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        try
        {
            rule.Validate(index, context);
        }
        catch (Exception ex)
        {
            context.Error(
                "IDX999",
                $"Integrity rule '{rule.GetType().Name}' failed with an unexpected exception.",
                ex.Message);
        }
    }
}