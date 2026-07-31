namespace X7.Knowledge.Acquisition;

/// <summary>Escolhe o leitor pelo formato do arquivo de solução.</summary>
public static class SolutionReader
{
    public static SolutionFile Read(string solutionPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solutionPath);

        if (!File.Exists(solutionPath))
            throw new FileNotFoundException(BuildNotFoundMessage(solutionPath), solutionPath);

        var extension = Path.GetExtension(solutionPath);

        return extension.ToLowerInvariant() switch
        {
            ".slnx" => SlnxReader.Read(solutionPath),
            ".sln" => SlnReader.Read(solutionPath),
            _ => throw new NotSupportedException(
                $"Formato de solução não suportado: '{extension}'. Suportados: .sln, .slnx.")
        };
    }

    /// <summary>
    /// Mensagem que diz o caminho tentado e o que existe por perto.
    /// Erro de caminho é o mais comum e o mais barato de diagnosticar bem.
    /// </summary>
    private static string BuildNotFoundMessage(string solutionPath)
    {
        var full = Path.GetFullPath(solutionPath);

        var directory = Path.GetDirectoryName(full);

        var message = $"Solução não encontrada: '{full}'.";

        if (directory is null || !Directory.Exists(directory))
            return message + $" O diretório '{directory}' também não existe.";

        var candidates = Directory
            .EnumerateFiles(directory, "*.slnx")
            .Concat(Directory.EnumerateFiles(directory, "*.sln"))
            .Select(Path.GetFileName)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 0)
            return message + " Nenhuma solução (.sln/.slnx) nesse diretório.";

        return message
               + " Encontradas nesse diretório: "
               + string.Join(", ", candidates)
               + ".";
    }
}
