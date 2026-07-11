namespace X7.ProjectIndexer.Core.Integrity.Rules;

using X7.ProjectIndexer.Core.Models;

public abstract class IntegrityValidationRule
{
    public virtual string Name => GetType().Name;

    public virtual string Category => "General";

    public void Validate(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        try
        {
            Execute(index, context);
        }
        catch (Exception ex)
        {
            context.Error(
                "IDX999",
                $"Rule '{Name}' failed unexpectedly: {ex.Message}");
        }
    }

    protected abstract void Execute(
        ProjectIndexOld index,
        IntegrityValidationContext context);
}