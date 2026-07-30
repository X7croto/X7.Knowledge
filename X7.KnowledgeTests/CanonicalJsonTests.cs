using X7.Knowledge.Serialization;
using Xunit;

namespace X7.KnowledgeTests;

public sealed class CanonicalJsonTests
{
    [Fact]
    public void Chaves_saem_ordenadas_independente_da_ordem_de_entrada()
    {
        var json = CanonicalJson.Object(
            ("zeta", CanonicalJson.Of("3")),
            ("alfa", CanonicalJson.Of("1")),
            ("meio", CanonicalJson.Of("2"))).Serialize();

        var alfa = json.IndexOf("alfa", StringComparison.Ordinal);
        var meio = json.IndexOf("meio", StringComparison.Ordinal);
        var zeta = json.IndexOf("zeta", StringComparison.Ordinal);

        Assert.True(alfa < meio && meio < zeta);
    }

    [Fact]
    public void Membro_nulo_e_omitido_e_nunca_escrito_como_null()
    {
        var json = CanonicalJson.Object(
            ("presente", CanonicalJson.Of("v")),
            ("ausente", null)).Serialize();

        Assert.DoesNotContain("ausente", json, StringComparison.Ordinal);
        Assert.DoesNotContain("null", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Termina_em_LF_e_nao_contem_CR()
    {
        var json = CanonicalJson.Object(("a", CanonicalJson.Of("b"))).Serialize();

        Assert.EndsWith("\n", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Colecao_vazia_e_estavel()
    {
        var json = CanonicalJson.Object(
            ("itens", CanonicalJson.Strings([]))).Serialize();

        Assert.Contains("\"itens\": []", json, StringComparison.Ordinal);
    }
}
