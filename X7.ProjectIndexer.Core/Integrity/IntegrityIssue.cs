namespace X7.ProjectIndexer.Core.Integrity;

public sealed class IntegrityIssue
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    public required IntegritySeverity Severity { get; init; }

    public string? Location { get; init; }
}