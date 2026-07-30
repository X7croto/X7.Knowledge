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
  x7k X7_ProjectIndexer.slnx
  x7k X7_ProjectIndexer.slnx -o Knowledge

SAÍDA
  <output>/README.md
  <output>/Structure/Solution.md
  <output>/model/knowledge.model.json

CÓDIGOS DE RETORNO
  0  compilação concluída
  1  erro de uso ou entrada inválida
  2  invariante do modelo violado
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

    var limitations = model.Observations
        .Count(o => o.Kind == ObservationKinds.AcquisitionLimitation);

    if (limitations > 0)
        Console.WriteLine($"Limitações   {limitations} (declaradas em Structure/Solution.md)");

    Console.WriteLine($"Digest       {manifest.InputDigest[..16]}…");
    Console.WriteLine();
    Console.WriteLine($"Base publicada em {options.OutputDirectory}");
    Console.WriteLine($"Tempo {stopwatch.ElapsedMilliseconds} ms");

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
catch (Exception ex) when (ex is FileNotFoundException
                              or NotSupportedException
                              or InvalidOperationException
                              or InvalidDataException)
{
    Console.Error.WriteLine($"Erro: {ex.Message}");
    return 1;
}
