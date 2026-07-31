using System.Diagnostics;
using X7.Knowledge;
using X7.Knowledge.Cli;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;

const string Usage = """
x7k — compilador de conhecimento X7.Knowledge

USO
  x7k [solução] [-o <diretório>]

ARGUMENTOS
  solução            Caminho do .slnx ou .sln.
                     Omitido: procura uma única solução no diretório atual.

OPÇÕES
  -o, --output       Diretório da Base publicada. Padrão: Knowledge
  -h, --help         Esta ajuda.

EXEMPLOS
  x7k
  x7k X7.ProjectIndexer.slnx
  x7k X7.ProjectIndexer.slnx -o Knowledge

SAÍDA
  <output>/README.md
  <output>/Structure/Solution.md
  <output>/model/knowledge.model.json

CÓDIGOS DE RETORNO
  0  compilação concluída
  1  erro de uso ou entrada inválida
  2  invariante do modelo violado
  3  erro de acesso a disco
""";

var options = Options.Parse(args, Console.Error);

if (options is null)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine(Usage);
    return 1;
}

try
{
    var stopwatch = Stopwatch.StartNew();

    var model = await KnowledgeCompiler.CompileAsync(
        options.SolutionPath,
        options.OutputDirectory);

    stopwatch.Stop();

    var manifest = model.Manifest;

    Console.WriteLine($"Solução      {model.Entities.Solution.Name}");
    Console.WriteLine($"Nível        {manifest.AcquisitionLevel.ToToken()} (sintático)");
    Console.WriteLine($"Capacidades  {string.Join(", ", manifest.Capabilities)}");
    Console.WriteLine($"Projetos     {model.Entities.Projects.Count}");
    Console.WriteLine($"Pastas       {model.Entities.Folders.Count}");
    Console.WriteLine($"Observations {manifest.ObservationCount}");
    Console.WriteLine($"Evidence     {manifest.EvidenceCount}");
    Console.WriteLine($"Inferences   {manifest.InferenceCount}");

    var limitations = model.Observations
        .Count(o => o.Kind == ObservationKinds.AcquisitionLimitation);

    if (limitations > 0)
        Console.WriteLine($"Limitações   {limitations} (declaradas em Structure/Solution.md)");

    Console.WriteLine($"Digest       {manifest.InputDigest[..16]}…");
    Console.WriteLine();
    Console.WriteLine($"Base publicada em {options.OutputDirectory}");
    Console.WriteLine($"Tempo {stopwatch.ElapsedMilliseconds} ms");

    var parent = Path.GetDirectoryName(options.OutputDirectory);
    var prefix = Path.GetFileName(options.OutputDirectory) + ".previous";

    var leftovers = parent is null
        ? []
        : Directory.EnumerateDirectories(parent, prefix + "*")
            .Order(StringComparer.Ordinal)
            .ToArray();

    if (leftovers.Length > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"Aviso: {leftovers.Length} pasta(s) de descarte não puderam ser removidas.");

        foreach (var leftover in leftovers)
            Console.WriteLine($"  {Path.GetFileName(leftover)}");

        Console.WriteLine("A Base publicada está correta. Apague-as quando puder.");
    }

    return 0;
}
catch (InvariantViolationException ex)
{
    Console.Error.WriteLine("Invariantes do KnowledgeModel violados. Nada foi publicado.");
    Console.Error.WriteLine();

    foreach (var violation in ex.Violations)
        Console.Error.WriteLine($"  - {violation}");

    return 2;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Erro de acesso a disco: {ex.Message}");
    Console.Error.WriteLine();
    Console.Error.WriteLine(
        "Causa comum: cliente de sincronização (Google Drive, OneDrive, Dropbox), "
        + "antivírus ou editor mantendo arquivos abertos no diretório de saída.");
    Console.Error.WriteLine(
        "Alternativas: pausar a sincronização durante a compilação, ou publicar "
        + "fora da pasta sincronizada com -o.");

    return 3;
}
catch (Exception ex) when (ex is FileNotFoundException
                              or NotSupportedException
                              or InvalidOperationException
                              or InvalidDataException)
{
    Console.Error.WriteLine($"Erro: {ex.Message}");
    return 1;
}
