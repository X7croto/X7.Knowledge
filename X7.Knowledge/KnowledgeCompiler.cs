using System.Reflection;
using X7.Knowledge.Acquisition;
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
    public const string ModelVersion = "0.1.0";

    public static async ValueTask<KnowledgeModel> CompileAsync(
        string solutionPath,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        var solution = SolutionReader.Read(solutionPath);

        // C01 é capacidade de nível X: nada aqui depende de análise semântica.
        var context = new CompilationContext(solution, AcquisitionLevel.Syntactic);

        var pipeline = new KnowledgePipeline(
        [
            new SolutionProducer(),
            new ProjectProducer()
        ]);

        await pipeline.ExecuteAsync(context, cancellationToken);

        var model = context.Knowledge.Build(
            ModelVersion,
            CompilerVersion(),
            context.AcquisitionLevel,
            ["C01"],
            InputDigest.Compute(solution));

        var violations = ModelInvariants.Validate(model);

        if (violations.Count > 0)
            throw new InvariantViolationException(violations);

        // Substituição integral: a Base nunca contradiz a solução (ADR-031).
        // Só apaga o que reconhece como Base própria — nunca uma pasta arbitrária.
        PrepareOutputDirectory(outputDirectory);

        IPublisher[] publishers =
        [
            new KnowledgeModelPublisher(),
            new MarkdownPublisher()
        ];

        foreach (var publisher in publishers)
            await publisher.PublishAsync(model, outputDirectory, cancellationToken);

        return model;
    }

    /// <summary>
    /// Limpa a saída anterior, mas recusa apagar pasta que não seja uma Base
    /// já publicada. Evita perda de dados por argumento errado.
    /// </summary>
    private static void PrepareOutputDirectory(string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
            return;

        var isEmpty = !Directory
            .EnumerateFileSystemEntries(outputDirectory)
            .Any();

        if (isEmpty)
            return;

        var marker = Path.Combine(outputDirectory, "model", "knowledge.model.json");

        if (!File.Exists(marker))
        {
            throw new InvalidOperationException(
                $"'{outputDirectory}' existe, não está vazio e não é uma Base publicada " +
                "(falta model/knowledge.model.json). Recusando apagar. " +
                "Escolha outro diretório de saída ou remova-o manualmente.");
        }

        Directory.Delete(outputDirectory, recursive: true);
    }

    private static string CompilerVersion()
        => typeof(KnowledgeCompiler).Assembly
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
               ?.InformationalVersion
           ?? typeof(KnowledgeCompiler).Assembly.GetName().Version?.ToString()
           ?? "0.0.0";
}
