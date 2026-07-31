using System.Text.Json;

namespace X7.Knowledge.Benchmark;

/// <summary>
/// Comparação pareada com uma medição anterior (BM-09).
/// </summary>
/// <remarks>
/// A comparabilidade é decidida **por pergunta**, não pela solução inteira.
/// Uma pergunta é comparável quando seu `T_code` não mudou: aí a diferença de
/// CR vem da Base, que é o que se quer medir. Se o `T_code` mudou, o
/// denominador é outro e a pergunta sai do cálculo.
///
/// A trava anterior era por digest da solução: qualquer edição em qualquer
/// `.csproj` — até um comentário — invalidava a medição inteira, mesmo para
/// perguntas que não liam aquele arquivo. Grosso demais para ser útil.
/// </remarks>
public sealed record BaselineComparison
{
    public required IReadOnlyList<string> CommonQuestions { get; init; }

    /// <summary>Sustentadas em ambas, mas com T_code diferente.</summary>
    public required IReadOnlyList<string> ExcludedQuestions { get; init; }

    public required int? MedianBeforePerMille { get; init; }

    public required int? MedianAfterPerMille { get; init; }

    /// <summary>
    /// Perguntas cujo CR piorou individualmente.
    /// </summary>
    /// <remarks>
    /// A mediana pode não se mover mesmo com uma resposta degradando: com
    /// poucas perguntas, uma piora nos extremos não desloca o centro. MT-02
    /// olha o agregado; isto expõe o caso a caso, que é onde o dano aparece.
    /// </remarks>
    public required IReadOnlyList<QuestionChange> Worsened { get; init; }

    public bool Comparable => CommonQuestions.Count > 0;

    public bool Regressed => MedianBeforePerMille is { } before
                             && MedianAfterPerMille is { } after
                             && after > before;

    private sealed record Previous(int PerMille, int CodeTokens);

    public sealed record QuestionChange
    {
        public required string Id { get; init; }

        public required int BeforePerMille { get; init; }

        public required int AfterPerMille { get; init; }
    }

    /// <summary>
    /// Em milésimos inteiros, que é a precisão em que o resultado é gravado.
    /// Comparar um valor arredondado contra um `double` de precisão cheia
    /// acusa piora onde não houve: 2.5513 é maior que 2.551.
    /// </summary>
    private static int PerMille(double ratio) => (int)Math.Round(ratio * 1000);

    public static BaselineComparison? Load(string path, IReadOnlyList<Measurement> current)
    {
        if (!File.Exists(path))
            return null;

        using var document = JsonDocument.Parse(File.ReadAllText(path));

        var before = new Dictionary<string, Previous>(StringComparer.Ordinal);

        if (document.RootElement.TryGetProperty("measurements", out var items))
        {
            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("contextRatioPerMille", out var ratio))
                    continue;

                if (item.TryGetProperty("broken", out var broken) && broken.GetBoolean())
                    continue;

                before[item.GetProperty("id").GetString()!] = new Previous(
                    ratio.GetInt32(),
                    item.GetProperty("codeTokens").GetInt32());
            }
        }

        var after = current
            .Where(m => m.ContextRatio is not null)
            .ToDictionary(m => m.Question.Id, m => m, StringComparer.Ordinal);

        var common = new List<string>();
        var excluded = new List<string>();

        foreach (var id in before.Keys.Intersect(after.Keys, StringComparer.Ordinal)
                                      .OrderBy(id => id, StringComparer.Ordinal))
        {
            if (before[id].CodeTokens == after[id].CodeTokens)
                common.Add(id);
            else
                excluded.Add(id);
        }

        var worsened = common
            .Select(id => new QuestionChange
            {
                Id = id,
                BeforePerMille = before[id].PerMille,
                AfterPerMille = PerMille(after[id].ContextRatio!.Value)
            })
            .Where(c => c.AfterPerMille > c.BeforePerMille)
            .OrderByDescending(c => c.AfterPerMille - c.BeforePerMille)
            .ThenBy(c => c.Id, StringComparer.Ordinal)
            .ToArray();

        return new BaselineComparison
        {
            CommonQuestions = common,
            ExcludedQuestions = excluded,
            MedianBeforePerMille = MedianOf(common.Select(id => before[id].PerMille)),
            MedianAfterPerMille = MedianOf(common.Select(id => PerMille(after[id].ContextRatio!.Value))),
            Worsened = worsened
        };
    }

    private static int? MedianOf(IEnumerable<int> values)
    {
        var ordered = values.Order().ToArray();

        if (ordered.Length == 0)
            return null;

        var middle = ordered.Length / 2;

        return ordered.Length % 2 == 1
            ? ordered[middle]
            : (ordered[middle - 1] + ordered[middle]) / 2;
    }
}
