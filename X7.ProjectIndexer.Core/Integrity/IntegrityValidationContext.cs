namespace X7.ProjectIndexer.Core.Integrity;

public sealed class IntegrityValidationContext
{
    private readonly List<IntegrityIssue> _issues = [];

    public IReadOnlyList<IntegrityIssue> Issues => _issues;

    public bool HasErrors =>
        _issues.Any(x => x.Severity == IntegritySeverity.Error);

    public void Error(
        string code,
        string message,
        string? location = null)
    {
        AddIssue(
            IntegritySeverity.Error,
            code,
            message,
            location);
    }

    public void Warning(
        string code,
        string message,
        string? location = null)
    {
        AddIssue(
            IntegritySeverity.Warning,
            code,
            message,
            location);
    }

    private void AddIssue(
        IntegritySeverity severity,
        string code,
        string message,
        string? location)
    {
        _issues.Add(new IntegrityIssue
        {
            Code = code,
            Message = message,
            Severity = severity,
            Location = location
        });
    }
}