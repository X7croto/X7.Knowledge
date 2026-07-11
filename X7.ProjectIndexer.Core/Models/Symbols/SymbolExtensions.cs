namespace X7.ProjectIndexer.Core.Models.Symbols;

using System.Reflection;

public static class SymbolExtensions
{
    public static string? GetStringProperty(
        this object symbol,
        string propertyName)
    {
        var prop = symbol.GetType()
            .GetProperty(propertyName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

        if (prop == null)
            return null;

        return prop.GetValue(symbol) as string;
    }

    public static object? GetProperty(
        this object symbol,
        string propertyName)
    {
        var prop = symbol.GetType()
            .GetProperty(propertyName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

        return prop?.GetValue(symbol);
    }

    public static bool HasProperty(
        this object symbol,
        string propertyName)
    {
        return symbol.GetType()
            .GetProperty(propertyName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase)
            != null;
    }
}