namespace X7.Knowledge.Acquisition;

/// <summary>
/// D-02: todo caminho na saída é relativo à raiz da solução, com separador '/'.
/// Caminho absoluto nunca aparece na saída.
/// </summary>
public static partial class PathNormalizer
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

    /// <summary>
    /// Remove caminhos absolutos de texto produzido por ferramentas externas.
    /// </summary>
    /// <remarks>
    /// Mensagem de diagnóstico do MSBuild vem com caminho absoluto da máquina.
    /// Publicá-la crua viola D-02 e IV-08 — e, pior, tornaria a saída
    /// dependente de onde a solução está no disco, quebrando PR-02.
    /// Toda mensagem externa passa por aqui antes de virar Observation.
    /// </remarks>
    public static string Sanitize(string message, string rootDirectory)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var cleaned = message
            .Replace(rootDirectory + Path.DirectorySeparatorChar, string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(rootDirectory, string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace('\\', '/');

        // Qualquer resto de caminho absoluto — de fora da solução — é elidido.
        cleaned = AbsolutePath().Replace(cleaned, "<caminho>");

        return cleaned;
    }

    [System.Text.RegularExpressions.GeneratedRegex(
        @"(?:[A-Za-z]:/|//)[^\s""'|]*",
        System.Text.RegularExpressions.RegexOptions.ExplicitCapture)]
    private static partial System.Text.RegularExpressions.Regex AbsolutePath();
}
