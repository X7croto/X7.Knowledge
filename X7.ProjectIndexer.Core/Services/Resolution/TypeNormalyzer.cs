namespace X7.ProjectIndexer.Core.Services.Resolution;

public static class TypeNameNormalizer
{
    public static string Normalize(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return string.Empty;

        var name = typeName.Trim();

        // Nullable reference/value types
        while (name.EndsWith("?"))
            name = name[..^1];

        // Remove argumentos genéricos
        var genericIndex = name.IndexOf('<');
        if (genericIndex >= 0)
            name = name[..genericIndex];

        return name.Trim();
    }
}