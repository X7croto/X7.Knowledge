using System.Reflection;
using X7.Knowledge.Acquisition;
using X7.Knowledge.Acquisition.Roslyn;
using X7.Knowledge.Compilation;
using X7.Knowledge.Compilation.Producers;
using X7.Knowledge.Model;
using X7.Knowledge.Publishing;

namespace X7.Knowledge;

/// <summary>
/// Ponto de entrada da compilação de conhecimento.
/// Cada compilação é função total da entrada e substitui integralmente
/// a saída anterior (PR-05, ADR-031).
/// </summary>
public static class KnowledgeCompiler
{
    public const string ModelVersion = "0.6.0";

    public static async ValueTask<KnowledgeModel> CompileAsync(
        string solutionPath,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        var solution = SolutionReader.Read(solutionPath);

        using var provider = new CompilationProvider();

        var sources = await provider.AcquireAsync(solution, cancellationToken);

        // Nível global é o menor alcançado: a Base não pode alegar semântica
        // que parte dela não tem. O nível por item fica na proveniência.
        var level = sources.Count > 0 && sources.All(s => s.Level == AcquisitionLevel.Semantic)
            ? AcquisitionLevel.Semantic
            : AcquisitionLevel.Syntactic;

        var context = new CompilationContext(solution, level);

        var pipeline = new KnowledgePipeline(
        [
            new SolutionProducer(),
            new ProjectProducer(),
            new ProjectReferenceProducer(),
            new ArchitectureProducer(),
            new CodeStructureProducer(sources),
            new TypeRelationProducer(sources)
        ]);

        await pipeline.ExecuteAsync(context, cancellationToken);

        var model = context.Knowledge.Build(
            ModelVersion,
            CompilerVersion(),
            context.AcquisitionLevel,
            ["C01", "C02", "C03", "C04"],
            InputDigest.Compute(solution),
            level == AcquisitionLevel.Semantic ? MsBuildBootstrap.Version : null);

        var violations = ModelInvariants.Validate(model);

        if (violations.Count > 0)
            throw new InvariantViolationException(violations);

        IPublisher[] publishers =
        [
            new KnowledgeModelPublisher(),
            new MarkdownPublisher(),
            new ArchitecturePublisher(),
            new StructurePublisher(),
            new RelationPublisher()
        ];

        // Substituição integral (ADR-031), mas nunca destrutiva antes da hora:
        // publica inteiro numa área de preparo e só então troca. Se qualquer
        // Publisher falhar, a Base anterior continua intacta no lugar.
        var staging = outputDirectory + ".staging";

        ResilientDirectory.Delete(staging);

        try
        {
            foreach (var publisher in publishers)
                await publisher.PublishAsync(model, staging, cancellationToken);

            Swap(staging, outputDirectory);
        }
        catch
        {
            ResilientDirectory.TryDelete(staging);

            throw;
        }

        return model;
    }

    /// <summary>
    /// Substitui o conteúdo da Base publicada pelo da área de preparo.
    /// </summary>
    /// <remarks>
    /// O diretório de saída nunca é movido nem apagado — apenas seu conteúdo.
    /// Cliente de sincronização mantém handle aberto na pasta que observa, e
    /// mover ou remover essa pasta falha de forma recorrente. Substituir
    /// arquivo a arquivo é mais lento e sobrevive ao ambiente real.
    ///
    /// A área de preparo permanece intacta até o fim: se a cópia falhar no
    /// meio, a Base completa continua lá e basta rodar de novo.
    /// </remarks>
    private static void Swap(string staging, string outputDirectory)
    {
        EnsureSafeToReplace(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        var publicados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var origem in Directory.EnumerateFiles(staging, "*", SearchOption.AllDirectories))
        {
            var relativo = Path.GetRelativePath(staging, origem);
            var destino = Path.Combine(outputDirectory, relativo);

            Directory.CreateDirectory(Path.GetDirectoryName(destino)!);

            ResilientDirectory.CopyFile(origem, destino);

            publicados.Add(relativo);
        }

        // Remove o que sobrou da Base anterior e não faz parte desta.
        foreach (var existente in Directory.EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories))
        {
            var relativo = Path.GetRelativePath(outputDirectory, existente);

            if (!publicados.Contains(relativo))
                ResilientDirectory.TryDeleteFile(existente);
        }

        RemoveEmptyDirectories(outputDirectory);

        // Sujeira não invalida publicação correta.
        ResilientDirectory.TryDelete(staging);
    }

    private static void RemoveEmptyDirectories(string root)
    {
        foreach (var directory in Directory
                     .EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                     .OrderByDescending(d => d.Length))
        {
            if (!Directory.EnumerateFileSystemEntries(directory).Any())
                ResilientDirectory.TryDelete(directory);
        }
    }

    /// <summary>
    /// Recusa substituir diretório que não seja uma Base publicada.
    /// Evita perda de dados por argumento errado.
    /// </summary>
    private static void EnsureSafeToReplace(string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
            return;

        var isEmpty = !Directory.EnumerateFileSystemEntries(outputDirectory).Any();

        if (isEmpty)
            return;

        var marker = Path.Combine(outputDirectory, "model", "knowledge.model.json");

        if (!File.Exists(marker))
        {
            throw new InvalidOperationException(
                $"'{outputDirectory}' existe, não está vazio e não é uma Base publicada " +
                "(falta model/knowledge.model.json). Recusando substituir. " +
                "Escolha outro diretório de saída ou remova-o manualmente.");
        }
    }

    private static string CompilerVersion()
        => typeof(KnowledgeCompiler).Assembly
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
               ?.InformationalVersion
           ?? typeof(KnowledgeCompiler).Assembly.GetName().Version?.ToString()
           ?? "0.0.0";
}
