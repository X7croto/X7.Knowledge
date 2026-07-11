namespace X7.ProjectIndexer.Core.Integrity.Rules;

using X7.ProjectIndexer.Core.Models;

public sealed class DuplicateMemberReferenceRule : IntegrityValidationRule
{
    public override string Category => "Structure";

    protected override void Execute(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        foreach (var type in index.Semantic.Types)
        {
            Validate(type.Methods.Select(m => m.Id), "method", type.Name, context, "IDX120");
            Validate(type.Properties.Select(p => p.Id), "property", type.Name, context, "IDX121");
            Validate(type.Fields.Select(f => f.Id), "field", type.Name, context, "IDX122");
        }
    }

    private static void Validate(
        IEnumerable<string> ids,
        string kind,
        string typeName,
        IntegrityValidationContext context,
        string code)
    {
        var dup = ids.GroupBy(x => x).FirstOrDefault(g => g.Count() > 1);

        if (dup != null)
        {
            context.Error(
                code,
                $"Type '{typeName}' contains duplicate {kind} reference '{dup.Key}'.");
        }
    }
}