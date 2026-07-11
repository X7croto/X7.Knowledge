using X7.Knowledge;
using Xunit;

namespace X7.KnowledgeTests;

public sealed class KnowledgeBuilderTests
{
    [Fact]
    public void Deve_criar_um_modelo_vazio()
    {
        var builder = new KnowledgeBuilder();

        var model = builder.Build();

        Assert.Empty(model.Identities);
    }

    [Fact]
    public void Deve_criar_um_modelo_com_uma_identity()
    {
        var builder = new KnowledgeBuilder();

        builder.AddIdentity("Cliente");

        var model = builder.Build();

        Assert.Single(model.Identities);
    }

    [Fact]
    public void Deve_reutilizar_identity_existente()
    {
        var builder = new KnowledgeBuilder();

        var a = builder.AddIdentity("Cliente");
        var b = builder.AddIdentity("Cliente");

        var model = builder.Build();

        Assert.Single(model.Identities);

        Assert.Same(a, b);
    }

    [Fact]
    public void Deve_lancar_excecao_para_identity_vazia()
    {
        Assert.Throws<ArgumentException>(
            () => Identity.Create(""));
    }
}