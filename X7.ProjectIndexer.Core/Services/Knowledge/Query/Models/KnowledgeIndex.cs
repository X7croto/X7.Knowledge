namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

public sealed class KnowledgeIndex
{
    public List<TypeIndex> Types { get; } = [];

    public List<MethodIndex> Methods { get; } = [];

    public List<NamespaceIndex> Namespaces { get; } = [];

    public List<FeatureIndex> Features { get; } = [];
}