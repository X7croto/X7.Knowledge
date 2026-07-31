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
  --baseline    results.json anterior, para comparação pareada (BM-09)

CÓDIGOS DE RETORNO
  0  medição concluída
  1  erro de uso ou entrada inválida
  4  medição inválida: arquivo da Base declarado e ausente
  5  regressão de CR na comparação pareada (MT-02)
""";

string? questionsPath = null, knowledgeRoot = null, root = null, output = null, baseline = null;

for (var i = 0; i < args.Length; i++)
{
    string? Next() => ++i < args.Length ? args[i] : null;

    switch (args[i])
    {
        case "--questions": questionsPath = Next(); break;
        case "--knowledge": knowledgeRoot = Next(); break;
        case "--root": root = Next(); break;
        case "--output": output = Next(); break;
        case "--baseline": baseline = Next(); break;
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

var (solutionDigest, projectCount) = ReadBaseIdentity(knowledgeRoot);

Directory.CreateDirectory(output);

var utf8NoBom = new UTF8Encoding(false);

File.WriteAllText(
    Path.Combine(output, "results.json"),
    BenchmarkRunner.ToJson(set, measurements, solutionDigest, projectCount).Serialize(),
    utf8NoBom);

File.WriteAllText(
    Path.Combine(output, "REPORT.md"),
    ReportWriter.Build(set, measurements).Replace("\r\n", "\n"),
    utf8NoBom);

var median = BenchmarkRunner.Median(measurements);
var supported = measurements.Count(m => m.Supported);
var broken = measurements.Count(m => m.Broken);

Console.WriteLine($"Perguntas    {measurements.Count}");
Console.WriteLine($"Sustentadas  {supported}");
Console.WriteLine($"Cobertura    {(measurements.Count == 0 ? 0 : supported * 100 / measurements.Count)}%");
Console.WriteLine($"Mediana CR   {(median is null ? "—" : $"{median.Value * 1000:F0}‰")}");
Console.WriteLine();
Console.WriteLine($"Resultado em {Path.GetFullPath(output)}");

if (baseline is not null)
{
    var comparison = BaselineComparison.Load(baseline, measurements, solutionDigest);

    Console.WriteLine();

    if (comparison is null)
    {
        Console.WriteLine($"Linha de base não encontrada: {Path.GetFullPath(baseline)}");
    }
    else if (!comparison.Comparable)
    {
        Console.WriteLine("Comparação impossível: solução de referência diferente (BM-07).");
        Console.WriteLine("Refaça a linha de base antes de comparar.");
    }
    else if (comparison.MedianBefore is null || comparison.MedianAfter is null)
    {
        Console.WriteLine("Comparação impossível: nenhuma pergunta sustentada em ambas as medições.");
    }
    else
    {
        Console.WriteLine($"Comparação pareada sobre {comparison.CommonQuestions.Count} pergunta(s):");
        Console.WriteLine($"  antes  {comparison.MedianBefore.Value * 1000:F0}‰");
        Console.WriteLine($"  depois {comparison.MedianAfter.Value * 1000:F0}‰");
        Console.WriteLine(comparison.Regressed
            ? "  REGRESSÃO — MT-02 bloqueia a conclusão da capacidade."
            : "  sem regressão.");

        if (comparison.Regressed)
            return 5;
    }
}

if (broken > 0)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"MEDIÇÃO INVÁLIDA: {broken} pergunta(s) apontam arquivos da Base que não existem.");
    Console.Error.WriteLine("Republique a Base e rode de novo. Não registre este resultado como linha de base.");

    return 4;
}

if (solutionDigest is null)
{
    Console.WriteLine();
    Console.WriteLine("Aviso: manifesto da Base não pôde ser lido.");
    Console.WriteLine("Sem solutionDigest, a comparação com medições anteriores não é confiável (BM-07).");
}

return 0;

/// <summary>
/// Lê digest das entradas e contagem de projetos direto do manifesto da Base.
/// Duas medições só são comparáveis se estes valores baterem.
/// </summary>
static (string? Digest, int Projects) ReadBaseIdentity(string knowledgeRoot)
{
    var path = Path.Combine(knowledgeRoot, "model", "knowledge.model.json");

    if (!File.Exists(path))
        return (null, 0);

    try
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        var digest = document.RootElement
            .GetProperty("manifest")
            .GetProperty("inputDigest")
            .GetString();

        var projects = document.RootElement
            .GetProperty("entities")
            .GetProperty("projects")
            .GetArrayLength();

        return (digest, projects);
    }
    catch (Exception e) when (e is JsonException or KeyNotFoundException or IOException)
    {
        return (null, 0);
    }
}
