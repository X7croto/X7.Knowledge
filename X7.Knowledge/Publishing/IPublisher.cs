using X7.Knowledge.Model;

namespace X7.Knowledge.Publishing;

/// <summary>
/// Materializa o KnowledgeModel em um formato. Nunca produz, infere
/// ou altera conhecimento (PR-06).
/// </summary>
public interface IPublisher
{
    ValueTask PublishAsync(
        KnowledgeModel model,
        string outputDirectory,
        CancellationToken cancellationToken);
}
