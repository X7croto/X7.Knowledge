namespace X7.Knowledge.Compilation;

/// <summary>
/// Adiciona conhecimento ao KnowledgeModel. Nunca modifica conhecimento
/// produzido por outro Producer (PR-05).
/// </summary>
public interface IProducer
{
    string Name { get; }

    string Capability { get; }

    ValueTask ProduceAsync(CompilationContext context, CancellationToken cancellationToken);
}
