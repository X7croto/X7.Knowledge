namespace X7.ProjectIndexer.Knowledge.ExportModels;

public sealed class TypeExport
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Namespace { get; init; }

    public required string Kind { get; init; }

    public int FanIn { get; init; }

    public int FanOut { get; init; }

    public double Instability { get; init; }

    public double Abstractness { get; init; }

    public double Distance { get; init; }

    public int Layer { get; init; }
}