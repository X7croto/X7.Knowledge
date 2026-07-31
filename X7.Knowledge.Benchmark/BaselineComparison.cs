using System.Text.Json;

namespace X7.Knowledge.Benchmark;

/// <summary>
/// Comparação pareada com uma medição anterior (BM-09).
/// Medianas de conjuntos diferentes não são comparáveis: quando a cobertura
/// muda, a mediana global é calculada sobre populações distintas.
/// </summary>
public sealed record BaselineComparison
{
    public required string? BaselineSolutionDigest { get; init; }

    public required bool Comparable { get; init; }

    public required IReadOnlyList<string> CommonQuestions { get; init; }

    public required double? MedianBefore { get; init; }

    public required double? MedianAfter { get; init; }

    public bool Regressed => MedianBefore is { } before
                             && MedianAfter is { } after
                             && after > before;

    public static BaselineComparison? Load(
        string path,
        IReadOnlyList<Measurement> current,
        string? currentSolutionDigest)
    {
        if (!File.Exists(path))
            return null;

        using var document = JsonDocument.Parse(File.ReadAllText(path));

        var root = document.RootElement;

        var baselineDigest = root.TryGetProperty("solutionDigest", out var digest)
            ? digest.GetString()
            : null;

        var before = new Dictionary<string, double>(StringComparer.Ordinal);

        if (root.TryGetProperty("measurements", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("contextRatioPerMille", out var ratio))
                    continue;

                if (item.TryGetProperty("broken", out var broken) && broken.GetBoolean())
                    continue;

                before[item.GetProperty("id").GetString()!] = ratio.GetInt32() / 1000.0;
            }
        }

        var after = current
            .Where(m => m.ContextRatio is not null)
            .ToDictionary(m => m.Question.Id, m => m.ContextRatio!.Value, StringComparer.Ordinal);

        var common = before.Keys
            .Intersect(after.Keys, StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        return new BaselineComparison
        {
            BaselineSolutionDigest = baselineDigest,
            // BM-07: solução diferente torna a comparação sem sentido.
            Comparable = baselineDigest is not null
                         && currentSolutionDigest is not null
                         && string.Equals(baselineDigest, currentSolutionDigest, StringComparison.Ordinal),
            CommonQuestions = common,
            MedianBefore = MedianOf(common.Select(id => before[id])),
            MedianAfter = MedianOf(common.Select(id => after[id]))
        };
    }

    private static double? MedianOf(IEnumerable<double> values)
    {
        var ordered = values.OrderBy(v => v).ToArray();

        if (ordered.Length == 0)
            return null;

        var middle = ordered.Length / 2;

        return ordered.Length % 2 == 1
            ? ordered[middle]
            : (ordered[middle - 1] + ordered[middle]) / 2;
    }
}
