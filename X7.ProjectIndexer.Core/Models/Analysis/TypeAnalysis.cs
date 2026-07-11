using X7.ProjectIndexer.Core.Models.Symbols;

public sealed class TypeAnalysis
{
    public required TypeSymbol Type { get; init; }

    public int FanIn { get; set; }

    public int FanOut { get; set; }

    public double Instability { get; set; }

    public double Abstractness { get; set; }

    public double Distance { get; set; }

    public int Layer { get; set; }
}