namespace X7.Knowledge.Benchmark;

/// <summary>
/// Contagem aproximada e determinística, declarada em BENCHMARK.md §3.
/// Aplicada identicamente aos dois lados da razão, preserva o CR.
/// Não pretende reproduzir o tokenizador de nenhum modelo.
/// </summary>
public static class TokenCounter
{
    public static int Count(string content)
    {
        var tokens = 0;
        var inWord = false;

        foreach (var c in content)
        {
            if (char.IsWhiteSpace(c))
            {
                inWord = false;
                continue;
            }

            if (char.IsLetterOrDigit(c) || c == '_')
            {
                if (!inWord)
                {
                    tokens++;
                    inWord = true;
                }

                continue;
            }

            // Pontuação conta como token próprio.
            tokens++;
            inWord = false;
        }

        return tokens;
    }

    public static int CountFile(string path)
        => File.Exists(path) ? Count(File.ReadAllText(path)) : 0;
}
