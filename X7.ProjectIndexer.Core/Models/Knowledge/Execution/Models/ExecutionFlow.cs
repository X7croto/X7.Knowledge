using X7.ProjectIndexer.Core.Models.Knowledge.Execution.Models;

public sealed class ExecutionFlow
{
    public List<RequestFlow> Requests { get; } = [];

    public List<Pipeline> Pipelines { get; } = [];

    public List<EventFlow> Events { get; } = [];

    public List<TransactionFlow> Transactions { get; } = [];

    public List<BackgroundFlow> BackgroundJobs { get; } = [];

    public DependencyInjectionMap DI { get; } = new();
}