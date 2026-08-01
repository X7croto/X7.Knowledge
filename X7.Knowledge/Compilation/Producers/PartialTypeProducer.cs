using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// C04 — conclui que um tipo é `partial` a partir de seus locais de
/// declaração. Não observa nada: consome apenas Observations já produzidas.
/// </summary>
/// <remarks>
/// A implicação é exata num sentido só. Mais de um local de declaração
/// implica `partial`, e por isso a Confidence é `Asserted`. A recíproca é
/// falsa: `partial` declarado num único arquivo existe, é comum, e esta regra
/// não o detecta. Ausência da Inference significa <b>não detectado</b>, nunca
/// <b>não parcial</b> — e é por isso que o Producer registra a limitação
/// correspondente em toda compilação.
///
/// Nota de revisão (KNOWLEDGE_MODEL §6.3.2): `partial` é modificador
/// sintático e está disponível na declaração, que este compilador já lê para
/// obter os demais modificadores. Observá-lo diretamente eliminaria a
/// limitação. A troca custa remoção de kind — versão maior e ADR (EX-03).
/// </remarks>
public sealed class PartialTypeProducer : IProducer
{
    private const string Rule = "partial-by-multiple-declaration-sites";

    public string Name => nameof(PartialTypeProducer);

    public string Capability => "C04";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        var locations = context.Knowledge.Observations
            .Where(o => o.Kind == ObservationKinds.TypeLocation)
            .GroupBy(o => o.Subject)
            .Where(g => g.Count() > 1)
            .OrderBy(g => g.Key)
            .ToArray();

        // A limitação é declarada mesmo quando nenhum tipo parcial é
        // detectado: o que ela informa é o alcance da regra, não o resultado
        // desta compilação. Ausência silenciosa é proibida.
        context.Knowledge.Add(
            ObservationKinds.AcquisitionLimitation,
            context.SolutionId,
            ObservationPayload.From(
                ("reason",
                    "Tipo `partial` declarado em um único arquivo não é detectado; "
                    + "a regra deriva parcialidade de múltiplos locais de declaração"),
                ("affectedScope", "type-partial-single-site")),
            new Provenance
            {
                Source = context.Solution.FileName,
                Producer = Name,
                Capability = Capability,
                AcquisitionLevel = context.AcquisitionLevel
            });

        foreach (var group in locations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var evidence = context.Knowledge.AddEvidence(
                Evidence.Create(
                    EvidenceKinds.TypeDeclarationSites,
                    group.Select(o => o.Id),
                    Name,
                    Capability));

            context.Knowledge.AddInference(Inference.Create(
                InferenceKinds.TypeIsPartial,
                group.Key,
                ObservationPayload.Empty,
                evidence,
                Confidence.Asserted,
                new InferenceProvenance
                {
                    Rule = Rule,
                    Producer = Name,
                    Capability = Capability,
                    AcquisitionLevel = context.AcquisitionLevel
                }));
        }

        return ValueTask.CompletedTask;
    }
}
