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
    public const string ModelVersion = "0.3.0";

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
            new ProjectProducer(),
            new ProjectReferenceProducer(),
            new ArchitectureProducer()
        ]);

        await pipeline.ExecuteAsync(context, cancellationToken);

        var model = context.Knowledge.Build(
            ModelVersion,
            CompilerVersion(),
            context.AcquisitionLevel,
            ["C01", "C02"],
            InputDigest.Compute(solution));

        var violations = ModelInvariants.Validate(model);

        if (violations.Count > 0)
            throw new InvariantViolationException(violations);

        IPublisher[] publishers =
        [
            new KnowledgeModelPublisher(),
            new MarkdownPublisher(),
            new ArchitecturePublisher()
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
    /// Troca a Base publicada pela recém-preparada.
    /// A anterior só é removida depois que a nova está inteira em disco.
    /// </summary>
    private static void Swap(string staging, string outputDirectory)
    {
        EnsureSafeToReplace(outputDirectory);

        var discarded = FindDiscardPath(outputDirectory);

        var hadPrevious = Directory.Exists(outputDirectory);

        if (hadPrevious)
            ResilientDirectory.Move(outputDirectory, discarded);

        try
        {
            ResilientDirectory.Move(staging, outputDirectory);
        }
        catch
        {
            // Falhou a troca: devolve a Base anterior ao lugar.
            if (hadPrevious && !Directory.Exists(outputDirectory))
                ResilientDirectory.Move(discarded, outputDirectory);

            throw;
        }

        // A Base nova já está publicada. Não conseguir apagar o descarte é
        // sujeira, não falha: quebrar aqui destruiria um resultado válido.
        ResilientDirectory.TryDelete(discarded);
    }


    /// <summary>
    /// Nome livre para a Base descartada. Se `.previous` não puder ser
    /// removida — trava de sincronização é o caso comum — usa `.previous.1`,
    /// `.previous.2` e assim por diante.
    /// </summary>
    /// <remarks>
    /// Não conseguir apagar sobra antiga não pode impedir uma publicação
    /// válida. Sujeira em disco é problema menor que Base desatualizada:
    /// o benchmark mede a Base publicada, e Base velha produz número errado.
    /// </remarks>
    private static string FindDiscardPath(string outputDirectory)
    {
        if (IsAvailable(outputDirectory + ".previous"))
            return outputDirectory + ".previous";

        for (var suffix = 1; suffix <= 64; suffix++)
        {
            var candidate = $"{outputDirectory}.previous.{suffix}";

            if (IsAvailable(candidate))
                return candidate;
        }

        throw new IOException(
            $"Não há nome livre para descartar a Base anterior em '{outputDirectory}'. " +
            "Remova as pastas '.previous*' manualmente.");
    }


    /// <summary>
    /// Nome está livre se nada existe ali, ou se era um diretório de descarte
    /// que conseguimos remover. Arquivo ocupando o nome nunca é apagado —
    /// pode ser dado do usuário — apenas evitado.
    /// </summary>
    private static bool IsAvailable(string path)
    {
        if (File.Exists(path))
            return false;

        return !Directory.Exists(path) || ResilientDirectory.TryDelete(path);
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
