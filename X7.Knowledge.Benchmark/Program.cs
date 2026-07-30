using System.Text;
using System.Text.Json;
using X7.Knowledge.Benchmark;

const string Usage = """
x7k-bench — mede o Context Ratio (Constituição §7)

USO
  x7k-bench --questions <arquivo> --knowledge <dir> [--root <dir>] [--output <dir>]

OPÇÕES
  --questions   questions.json com o conjunto versionado
  --knowledge   Base publicada. Ex.: Knowledge
  --root        Raiz da solução de referência. Padrão: diretório atual
  --output      Onde gravar results.json e REPORT.md. Padrão: benchmark/results
""";

string? questionsPath = null, knowledgeRoot = null, root = null, output = null;

for (var i = 0; i < args.Length; i++)
{
    string? Next() => ++i < args.Length ? args[i] : null;

    switch (args[i])
    {
        case "--questions": questionsPath = Next(); break;
        case "--knowledge": knowledgeRoot = Next(); break;
        case "--root": root = Next(); break;
        case "--output": output = Next(); break;
        default:
            Console.Error.WriteLine($"Opção desconhecida: {args[i]}");
            Console.Error.WriteLine(Usage);
            return 1;
    }
}

if (questionsPath is null || knowledgeRoot is null)
{
    Console.Error.WriteLine(Usage);
    return 1;
}

root ??= Directory.GetCurrentDirectory();
output ??= Path.Combine("benchmark", "results");

if (!File.Exists(questionsPath))
{
    Console.Error.WriteLine($"Conjunto não encontrado: {Path.GetFullPath(questionsPath)}");
    return 1;
}

var set = JsonSerializer.Deserialize<QuestionSet>(
    File.ReadAllText(questionsPath),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

if (set is null)
{
    Console.Error.WriteLine("questions.json inválido.");
    return 1;
}

var measurements = BenchmarkRunner.Measure(set, root, knowledgeRoot);

Directory.CreateDirectory(output);

var utf8NoBom = new UTF8Encoding(false);

File.WriteAllText(
    Path.Combine(output, "results.json"),
    BenchmarkRunner.ToJson(set, measurements).Serialize(),
    utf8NoBom);

File.WriteAllText(
    Path.Combine(output, "REPORT.md"),
    ReportWriter.Build(set, measurements).Replace("\r\n", "\n"),
    utf8NoBom);

var median = BenchmarkRunner.Median(measurements);
var supported = measurements.Count(m => m.Supported);

Console.WriteLine($"Perguntas    {measurements.Count}");
Console.WriteLine($"Sustentadas  {supported}");
Console.WriteLine($"Cobertura    {(measurements.Count == 0 ? 0 : supported * 100 / measurements.Count)}%");
Console.WriteLine($"Mediana CR   {(median is null ? "—" : $"{median.Value * 1000:F0}‰")}");
Console.WriteLine();
Console.WriteLine($"Resultado em {Path.GetFullPath(output)}");

return 0;
