using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// ADR-041 — fronteira do que é observado: sob a raiz da solução, fora de
/// `bin/` e de `obj/`, nos dois níveis de aquisição.
/// </summary>
/// <remarks>
/// A fixture declara um `[GeneratedRegex]` de propósito. O gerador emite
/// tipos dentro de `obj/`, com `&lt;` e `&gt;` no nome — foi assim que a saída
/// de build entrou na Base publicada e ficou lá desde o C03, invisível até o
/// C05 tentar transformar aquele nome em caminho de arquivo.
/// </remarks>
public sealed class AcquisitionBoundaryTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public AcquisitionBoundaryTests(SolutionFixture fixture) => _fixture = fixture;

    private Task<KnowledgeModel> CompileAsync()
        => KnowledgeCompiler.CompileAsync(
            _fixture.SolutionPath,
            Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"))).AsTask();

    private static IEnumerable<string> Paths(KnowledgeModel model)
    {
        foreach (var observation in model.Observations)
        {
            yield return observation.Provenance.Source;

            var file = observation.Payload["file"];

            if (file is not null)
                yield return file;
        }
    }

    [Fact]
    public async Task Nenhum_arquivo_observado_vem_de_saida_de_build()
    {
        var model = await CompileAsync();

        var paths = Paths(model).ToArray();

        Assert.NotEmpty(paths);

        Assert.All(paths, p =>
        {
            Assert.DoesNotContain("/obj/", p, StringComparison.Ordinal);
            Assert.DoesNotContain("/bin/", p, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// IV-08. O caso que passou em produção era `C:/Users/…`: depois da
    /// normalização de D-02 o caminho perde a barra invertida, e a
    /// verificação original não o reconhecia.
    /// </summary>
    [Fact]
    public async Task Nenhum_caminho_publicado_e_enraizado()
    {
        var model = await CompileAsync();

        Assert.Empty(ModelInvariants.Validate(model));

        Assert.All(Paths(model), p =>
        {
            Assert.False(Path.IsPathRooted(p), p);
            Assert.DoesNotContain("..", p, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Tipo_emitido_por_gerador_de_codigo_nao_entra_na_Base()
    {
        var model = await CompileAsync();

        var nomes = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeDeclared)
            .Select(o => o.Payload["metadataName"] ?? string.Empty)
            .ToArray();

        Assert.NotEmpty(nomes);

        // O tipo escrito à mão continua lá; o que o gerador emitiu a partir
        // dele, não.
        Assert.Contains(nomes, n => n.Contains("PathRules", StringComparison.Ordinal));

        // `<` em nome de tipo é legítimo: é a lista de parâmetros genéricos,
        // e ali ele vem sempre depois de um identificador. O que denuncia
        // nome emitido pelo compilador é o `<` na posição de nome — no
        // início, ou logo depois de um ponto.
        Assert.All(nomes, n =>
        {
            Assert.False(
                n.StartsWith('<') || n.Contains(".<", StringComparison.Ordinal),
                $"Nome emitido por gerador na Base: {n}");

            Assert.DoesNotContain("RegexGenerator", n, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// O nível declara quanto o compilador resolve, não o que ele enxerga.
    /// Antes da ADR-041, S e X produziam conjuntos de tipos diferentes para a
    /// mesma solução, e nada percebia.
    /// </summary>
    [Fact]
    public async Task Namespace_de_gerador_nao_aparece_na_hierarquia()
    {
        var model = await CompileAsync();

        var namespaces = model.Observations
            .Where(o => o.Kind == ObservationKinds.NamespaceDeclared)
            .Select(o => o.Payload["name"] ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(namespaces, n =>
            n.EndsWith(".Generated", StringComparison.Ordinal));
    }
}
