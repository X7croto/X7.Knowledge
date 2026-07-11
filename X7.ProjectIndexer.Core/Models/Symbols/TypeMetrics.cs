namespace X7.ProjectIndexer.Core.Models.Analysis;

using X7.ProjectIndexer.Core.Models.Symbols;

public sealed class TypeMetrics
{
    public required TypeSymbol Type { get; init; }

    public int AfferentCoupling { get; set; }

    public int EfferentCoupling { get; set; }

    public double Instability { get; set; }

    public double Abstractness { get; set; }

    public double Distance { get; set; }

    public int Layer { get; set; }
}