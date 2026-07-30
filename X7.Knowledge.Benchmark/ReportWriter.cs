using System.Globalization;
using System.Text;

namespace X7.Knowledge.Benchmark;

public static class ReportWriter
{
    public static string Build(QuestionSet set, IReadOnlyList<Measurement> measurements)
    {
        var builder = new StringBuilder();

        var median = BenchmarkRunner.Median(measurements);
        var supported = measurements.Count(m => m.Supported);

        builder.Append("# Context Ratio — resultado\n\n");
        builder.Append($"Conjunto v{set.BenchmarkVersion} · referência `{set.ReferenceSolution}`\n\n");

        builder.Append("| Métrica | Valor |\n|---|---|\n");
        builder.Append($"| Perguntas | {measurements.Count} |\n");
        builder.Append($"| Sustentadas pela Base | {supported} |\n");
        builder.Append($"| Cobertura | {Percent(measurements.Count == 0 ? 0 : (double)supported / measurements.Count)} |\n");
        builder.Append($"| **Mediana de CR** | **{(median is null ? "—" : Ratio(median.Value))}** |\n\n");

        builder.Append("## Por pergunta\n\n");
        builder.Append("| ID | Capacidade | Sustentada | T_code | T_kb | CR |\n");
        builder.Append("|---|---|---|---|---|---|\n");

        foreach (var m in measurements)
        {
            builder.Append($"| {m.Question.Id} ")
                   .Append($"| {m.Question.ExpectedCapability} ")
                   .Append($"| {(m.Supported ? "sim" : "**não**")} ")
                   .Append($"| {m.CodeTokens} ")
                   .Append($"| {(m.Supported ? m.KbTokens.ToString(CultureInfo.InvariantCulture) : "—")} ")
                   .Append($"| {(m.ContextRatio is null ? "—" : Ratio(m.ContextRatio.Value))} |\n");
        }

        var unsupported = measurements.Where(m => !m.Supported).ToArray();

        if (unsupported.Length > 0)
        {
            builder.Append("\n## Não sustentadas\n\n");
            builder.Append("Contam como falha (MT-03), não como CR baixo. ");
            builder.Append("Cada uma indica a capacidade que precisa existir.\n\n");

            foreach (var m in unsupported)
                builder.Append($"- **{m.Question.Id}** ({m.Question.ExpectedCapability}) — {m.Question.Text}\n");
        }

        var withMissing = measurements.Where(m => m.MissingCodeFiles.Count > 0).ToArray();

        if (withMissing.Length > 0)
        {
            builder.Append("\n## Arquivos declarados e ausentes\n\n");
            builder.Append("Medição comprometida: `codeFiles` aponta para arquivo inexistente.\n\n");

            foreach (var m in withMissing)
            {
                foreach (var file in m.MissingCodeFiles)
                    builder.Append($"- {m.Question.Id}: `{file}`\n");
            }
        }

        return builder.ToString();
    }

    private static string Ratio(double value)
        => (value * 1000).ToString("F0", CultureInfo.InvariantCulture) + "‰";

    private static string Percent(double value)
        => (value * 100).ToString("F0", CultureInfo.InvariantCulture) + "%";
}
