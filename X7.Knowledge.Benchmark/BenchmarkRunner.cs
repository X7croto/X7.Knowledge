using X7.Knowledge.Serialization;

namespace X7.Knowledge.Benchmark;

public sealed class BenchmarkRunner
{
    /// <param name="baseCapabilities">
    /// Capacidades executadas na Base medida, lidas do manifesto. Nulo
    /// significa manifesto ilegível: nesse caso nenhuma pergunta é
    /// considerada fora de escopo, e o comportamento é o anterior.
    /// </param>
    public static IReadOnlyList<Measurement> Measure(
        QuestionSet set,
        string solutionRoot,
        string knowledgeRoot,
        IReadOnlyCollection<string>? baseCapabilities = null)
    {
        var results = new List<Measurement>();

        foreach (var question in set.Questions.Where(q => !q.Retired)
                                              .OrderBy(q => q.Id, StringComparer.Ordinal))
        {
            var outOfScope = baseCapabilities is not null
                             && !baseCapabilities.Contains(
                                    question.ExpectedCapability,
                                    StringComparer.Ordinal);

            var missing = new List<string>();
            var codeTokens = 0;

            foreach (var relative in question.CodeFiles)
            {
                var path = Path.Combine(solutionRoot, relative.Replace('/', Path.DirectorySeparatorChar));

                if (!File.Exists(path))
                {
                    missing.Add(relative);
                    continue;
                }

                codeTokens += TokenCounter.CountFile(path);
            }

            var missingKb = new List<string>();
            var kbTokens = 0;

            // Fora de escopo não conta token nem arquivo faltando: a Base
            // nunca prometeu responder isso.
            if (!outOfScope)
            {
                foreach (var relative in question.KbFiles)
                {
                    var path = Path.Combine(
                        knowledgeRoot,
                        relative.Replace('/', Path.DirectorySeparatorChar));

                    if (!File.Exists(path))
                    {
                        missingKb.Add(relative);
                        continue;
                    }

                    kbTokens += TokenCounter.CountFile(path);
                }
            }

            results.Add(new Measurement
            {
                Question = question,
                CodeTokens = codeTokens,
                KbTokens = kbTokens,
                MissingCodeFiles = missing,
                MissingKbFiles = missingKb,
                OutOfScope = outOfScope
            });
        }

        return results;
    }

    public static double? Median(IReadOnlyList<Measurement> measurements)
    {
        var ratios = measurements
            .Select(m => m.ContextRatio)
            .Where(r => r is not null)
            .Select(r => r!.Value)
            .OrderBy(r => r)
            .ToArray();

        if (ratios.Length == 0)
            return null;

        var middle = ratios.Length / 2;

        return ratios.Length % 2 == 1
            ? ratios[middle]
            : (ratios[middle - 1] + ratios[middle]) / 2;
    }

    /// <summary>
    /// Razão em milésimos, inteiro. Evita ponto flutuante na saída canônica:
    /// formatação de double é a porta de entrada clássica para não determinismo.
    /// </summary>
    private static int PerMille(double value) => (int)Math.Round(value * 1000);

    public static CanonicalJson ToJson(
        QuestionSet set,
        IReadOnlyList<Measurement> measurements,
        string? solutionDigest,
        int projectCount)
    {
        var median = Median(measurements);
        var supported = measurements.Count(m => m.Supported);
        var broken = measurements.Count(m => m.Broken);

        return CanonicalJson.Object(
            ("benchmarkVersion", CanonicalJson.Of(set.BenchmarkVersion)),
            ("referenceSolution", CanonicalJson.Of(set.ReferenceSolution)),
            // BM-07: sem isso, duas medições incomparáveis parecem comparáveis.
            ("solutionDigest", solutionDigest is null ? null : CanonicalJson.Of(solutionDigest)),
            ("projectCount", CanonicalJson.Of(projectCount)),
            ("questionCount", CanonicalJson.Of(measurements.Count)),
            ("supportedCount", CanonicalJson.Of(supported)),
            ("brokenCount", CanonicalJson.Of(broken)),
            ("coveragePerMille", CanonicalJson.Of(
                measurements.Count == 0 ? 0 : PerMille((double)supported / measurements.Count))),
            ("medianContextRatioPerMille", median is null
                ? null
                : CanonicalJson.Of(PerMille(median.Value))),
            ("measurements", CanonicalJson.Array(measurements.Select(m =>
                CanonicalJson.Object(
                    ("id", CanonicalJson.Of(m.Question.Id)),
                    ("expectedCapability", CanonicalJson.Of(m.Question.ExpectedCapability)),
                    ("supported", CanonicalJson.Of(m.Supported)),
                    ("broken", m.Broken ? CanonicalJson.Of(true) : null),
                    ("missingKbFiles", m.MissingKbFiles.Count == 0
                        ? null
                        : CanonicalJson.Strings(m.MissingKbFiles)),
                    ("codeTokens", CanonicalJson.Of(m.CodeTokens)),
                    ("kbTokens", CanonicalJson.Of(m.KbTokens)),
                    ("contextRatioPerMille", m.ContextRatio is null
                        ? null
                        : CanonicalJson.Of(PerMille(m.ContextRatio.Value))),
                    ("missingCodeFiles", m.MissingCodeFiles.Count == 0
                        ? null
                        : CanonicalJson.Strings(m.MissingCodeFiles)))))));
    }
}
