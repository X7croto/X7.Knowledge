namespace X7.Knowledge.Compilation;

/// <summary>Executa os Producers em ordem declarada e estável.</summary>
public sealed class KnowledgePipeline
{
    private readonly IReadOnlyList<IProducer> _producers;

    public KnowledgePipeline(IReadOnlyList<IProducer> producers)
        => _producers = producers;

    public async ValueTask ExecuteAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        foreach (var producer in _producers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await producer.ProduceAsync(context, cancellationToken);
        }
    }
}
