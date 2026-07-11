namespace X7.ProjectIndexer.Core.Models.Symbols;

public sealed class TypeSymbol : BaseSymbol
{
    public string Namespace { get; init; } = "";

    public string Kind { get; init; } = "";

    public string Accessibility { get; init; } = "";

    public bool Partial { get; init; }

    public bool Static { get; init; }

    public bool Abstract { get; init; }

    public bool Record { get; init; }

    public string? BaseType { get; init; }

    public List<string> Interfaces { get; } = [];

    public List<string> Attributes { get; } = [];

    public List<MethodSymbol> Methods { get; } = [];

    public List<PropertySymbol> Properties { get; } = [];

    public List<FieldSymbol> Fields { get; } = [];

    public int FanIn { get; set; }

    public int FanOut { get; set; }

    public int AfferentCoupling => FanIn;

    public int EfferentCoupling => FanOut;

    public double Instability =>
        (FanIn + FanOut) == 0
            ? 0
            : (double)FanOut / (FanIn + FanOut);

    public bool IsLeaf => FanOut == 0;

    public bool IsRoot => FanIn == 0;

    public double Abstractness { get; set; }

    public double DistanceFromMainSequence { get; set; }

    public int Layer { get; set; } = -1;

    public TypeSymbol? BaseTypeSymbol { get; set; }

    public List<TypeSymbol> InterfaceSymbols { get; } = [];

    public string? BaseTypeQualifiedName { get; set; }

    public List<string> InterfaceQualifiedNames { get; } = [];

}