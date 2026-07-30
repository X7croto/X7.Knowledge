namespace X7.Knowledge.Acquisition;

/// <summary>Escolhe o leitor pelo formato do arquivo de solução.</summary>
public static class SolutionReader
{
    public static SolutionFile Read(string solutionPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solutionPath);

        if (!File.Exists(solutionPath))
            throw new FileNotFoundException("Solução não encontrada.", solutionPath);

        var extension = Path.GetExtension(solutionPath);

        return extension.ToLowerInvariant() switch
        {
            ".slnx" => SlnxReader.Read(solutionPath),
            ".sln" => SlnReader.Read(solutionPath),
            _ => throw new NotSupportedException(
                $"Formato de solução não suportado: '{extension}'. Suportados: .sln, .slnx.")
        };
    }
}
