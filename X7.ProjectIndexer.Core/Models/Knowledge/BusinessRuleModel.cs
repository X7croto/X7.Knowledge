namespace X7.ProjectIndexer.Core.Models.Knowledge;

public sealed class BusinessRuleModel
{
    public required string Name { get; init; }

    public required string MethodId { get; init; }

    public string? Description { get; set; }

    public List<string> Dependencies { get; } = [];
}