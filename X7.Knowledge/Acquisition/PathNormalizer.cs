namespace X7.Knowledge.Acquisition;

/// <summary>
/// D-02: todo caminho na saída é relativo à raiz da solução, com separador '/'.
/// Caminho absoluto nunca aparece na saída.
/// </summary>
public static class PathNormalizer
{
    public static string ToRelative(string root, string absolutePath)
    {
        var relative = Path.GetRelativePath(root, absolutePath);

        return Normalize(relative);
    }

    public static string Normalize(string path)
    {
        var normalized = path
            .Replace('\\', '/')
            .Trim('/');

        return normalized.Length == 0 ? "." : normalized;
    }

    /// <summary>Diretório de um caminho relativo, ou "." na raiz.</summary>
    public static string DirectoryOf(string relativePath)
    {
        var index = relativePath.LastIndexOf('/');

        return index < 0 ? "." : relativePath[..index];
    }
}
