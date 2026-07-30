using System.Text;

namespace X7.Knowledge.Publishing;

/// <summary>Escrita de arquivo em forma canônica: UTF-8 sem BOM, LF (D-06).</summary>
internal static class CanonicalFile
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static async ValueTask WriteAsync(
        string path,
        string content,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var normalized = content.Replace("\r\n", "\n").Replace("\r", "\n");

        await File.WriteAllTextAsync(path, normalized, Utf8NoBom, cancellationToken);
    }
}
