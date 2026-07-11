namespace X7.ProjectIndexer.Core.Integrity.Rules;

using X7.ProjectIndexer.Core.Models;

public sealed class MethodScopeConsistencyRule : IntegrityValidationRule
{
    public override string Category => "Scope";

    protected override void Execute(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        foreach (var scope in index.Semantic.ScopesByMethodId.Values)
        {
            if (scope.Method is null)
            {
                context.Error(
                    "IDX110",
                    "Scope without method reference.");
                continue;
            }

            if (!index.Semantic.Methods.Contains(scope.Method))
            {
                context.Error(
                    "IDX111",
                    $"Scope references unknown method '{scope.Method.Name}'.");
            }
        }
    }
}