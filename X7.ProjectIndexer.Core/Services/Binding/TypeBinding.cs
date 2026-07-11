public sealed class TypeReference
{
    public string OriginalText { get; set; } = "";

    public string? QualifiedName { get; set; }

    public bool Resolved { get; set; }

    public bool Ambiguous { get; set; }

    public List<string> Candidates { get; } = [];

    public List<TypeReference> GenericArguments { get; } = [];

    public bool IsArray { get; set; }

    public int ArrayRank { get; set; }

    public bool IsNullable { get; set; }

    public bool IsPointer { get; set; }

    public bool IsTuple { get; set; }

    public bool IsAlias { get; set; }

    public string? AliasName { get; set; }
}