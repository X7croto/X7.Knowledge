using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// C05, terceira fatia — restrições de parâmetro genérico (ADR-043).
/// </summary>
public sealed class GenericConstraintTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public GenericConstraintTests(SolutionFixture fixture) => _fixture = fixture;

    private string NewOutput() => Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

    private Task<KnowledgeModel> CompileAsync(string? output = null)
        => KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output ?? NewOutput()).AsTask();

    private static bool IsSemantic(KnowledgeModel model)
        => model.Manifest.AcquisitionLevel == AcquisitionLevel.Semantic;

    private static IReadOnlyList<Observation> Constraints(
        KnowledgeModel model,
        string kind,
        string subjectContains)
        => model.Observations
            .Where(o => o.Kind == kind
                        && o.Subject.Value.Contains(subjectContains, StringComparison.Ordinal))
            .OrderBy(o => int.Parse(o.Payload["ordinal"]!))
            .ToArray();

    [Fact]
    public async Task Invariantes_continuam_passando_com_as_restricoes()
    {
        var model = await CompileAsync();

        Assert.Empty(ModelInvariants.Validate(model));
    }

    /// <summary>
    /// `notnull` é analisado como restrição de tipo pelo compilador, mas não
    /// é tipo: não aparece em `ConstraintTypes`. Tratá-lo como tipo
    /// desalinharia o caminhamento e daria identidade a algo que não tem.
    /// </summary>
    [Fact]
    public async Task Restricao_de_palavra_chave_nao_recebe_identidade()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var restricao = Assert.Single(
            Constraints(model, ObservationKinds.TypeGenericConstraint, ".IQuery<"));

        Assert.Equal("T", restricao.Payload["parameter"]);
        Assert.Equal(MemberVocabulary.KeywordConstraint, restricao.Payload["form"]);
        Assert.Equal("notnull", restricao.Payload["value"]);
        Assert.Null(restricao.Payload["typeId"]);
    }

    [Fact]
    public async Task Restricao_de_tipo_da_solucao_recebe_identidade()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var restricoes = Constraints(model, ObservationKinds.MemberGenericConstraint, ".Order.Map");

        Assert.Equal(3, restricoes.Count);

        // A ordem é a escrita: `where T : class, IOrderPolicy, new()`.
        Assert.Equal(
            new[]
            {
                MemberVocabulary.KeywordConstraint,
                MemberVocabulary.TypeConstraint,
                MemberVocabulary.KeywordConstraint
            },
            restricoes.Select(o => o.Payload["form"]).ToArray());

        Assert.Equal("class", restricoes[0].Payload["value"]);
        Assert.Equal("new()", restricoes[2].Payload["value"]);

        Assert.Contains("IOrderPolicy", restricoes[1].Payload["value"]!, StringComparison.Ordinal);
        Assert.NotNull(restricoes[1].Payload["typeId"]);

        Assert.All(restricoes, o => Assert.Equal("T", o.Payload["parameter"]));
    }

    /// <summary>
    /// Um tipo pode declarar vários parâmetros e restringir só alguns.
    /// </summary>
    [Fact]
    public async Task Parametro_sem_restricao_nao_produz_Observation()
    {
        var model = await CompileAsync();

        if (!IsSemantic(model))
            return;

        var restricoes = Constraints(model, ObservationKinds.TypeGenericConstraint, ".IEvents<");

        var restrito = Assert.Single(restricoes);

        Assert.Equal("TIn", restrito.Payload["parameter"]);
        Assert.Equal("struct", restrito.Payload["value"]);
    }

    [Fact]
    public async Task Projecao_publica_a_clausula_como_a_linguagem_escreve()
    {
        var output = NewOutput();

        var model = await CompileAsync(output);

        if (!IsSemantic(model))
            return;

        var ordem = await File.ReadAllTextAsync(
            Path.Combine(output, "Behavior", "Domain", "Reference.Domain.Order.md"));

        Assert.Contains(
            "where T : class, IOrderPolicy, new()",
            ordem,
            StringComparison.Ordinal);

        var consulta = await File.ReadAllTextAsync(
            Path.Combine(output, "Behavior", "Domain", "Reference.Domain.IQuery-1.md"));

        Assert.Contains("where T : notnull", consulta, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Projecao_publica_o_valor_padrao_escrito()
    {
        var output = NewOutput();

        var model = await CompileAsync(output);

        if (!IsSemantic(model))
            return;

        var conteudo = await File.ReadAllTextAsync(
            Path.Combine(output, "Behavior", "Domain", "Reference.Domain.Ledger.md"));

        Assert.Contains("string rotulo = \"x\"", conteudo, StringComparison.Ordinal);
        Assert.Contains("int? escala = null", conteudo, StringComparison.Ordinal);
        Assert.Contains("ref readonly int destino", conteudo, StringComparison.Ordinal);

        // As reticências eram o sinal de que a assinatura estava incompleta.
        Assert.DoesNotContain("= …", conteudo, StringComparison.Ordinal);
    }
}
