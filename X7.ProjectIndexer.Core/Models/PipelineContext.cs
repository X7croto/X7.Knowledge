namespace X7.ProjectIndexer.Core.Services.Indexing;

using X7.ProjectIndexer.Core.Models;

public sealed class PipelineContext
{
    public required ProjectIndexOld Index { get; init; }

    public Dictionary<string, object> Stages { get; } = new();

    public void Set<T>(string key, T value) => Stages[key] = value!;
    public T Get<T>(string key) => (T)Stages[key];
    public bool Has(string key) => Stages.ContainsKey(key);
}