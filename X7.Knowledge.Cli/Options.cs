namespace X7.Knowledge.Cli;

internal sealed record Options
{
    public required string SolutionPath { get; init; }

    public required string OutputDirectory { get; init; }

    public static Options? Parse(string[] args, TextWriter error)
    {
        string? solution = null;
        string? output = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o" or "--output":
                    if (++i >= args.Length)
                    {
                        error.WriteLine("Erro: '-o' exige um caminho.");
                        return null;
                    }

                    output = args[i];
                    break;

                case "-h" or "--help":
                    return null;

                default:
                    if (args[i].StartsWith('-'))
                    {
                        error.WriteLine($"Erro: opção desconhecida '{args[i]}'.");
                        return null;
                    }

                    if (solution is not null)
                    {
                        error.WriteLine("Erro: mais de uma solução informada.");
                        return null;
                    }

                    solution = args[i];
                    break;
            }
        }

        solution ??= FindSolution(Directory.GetCurrentDirectory(), error);

        if (solution is null)
            return null;

        return new Options
        {
            SolutionPath = Path.GetFullPath(solution),
            OutputDirectory = Path.GetFullPath(output ?? "Knowledge")
        };
    }

    /// <summary>Sem argumento, procura uma única solução no diretório atual.</summary>
    private static string? FindSolution(string directory, TextWriter error)
    {
        var candidates = Directory
            .EnumerateFiles(directory, "*.slnx")
            .Concat(Directory.EnumerateFiles(directory, "*.sln"))
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 1)
            return candidates[0];

        error.WriteLine(candidates.Length == 0
            ? $"Erro: nenhuma solução (.sln/.slnx) em '{directory}'."
            : "Erro: mais de uma solução encontrada. Informe qual usar: "
              + string.Join(", ", candidates.Select(Path.GetFileName)));

        return null;
    }
}
