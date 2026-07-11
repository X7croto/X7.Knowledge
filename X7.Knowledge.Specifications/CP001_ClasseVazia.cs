using X7.Knowledge;
using Xunit;

namespace X7.KnowledgeTests.Capabilites;

public sealed class CP001_ClasseVazia
{
    [Fact]
    public void Deve_descobrir_uma_classe()
    {
        var model =
            KnowledgeEngine
                .Create()
                .Start()
                .Discover(
                    FakeSource.FromCode("""
                        class Cliente
                        {
                        }
                    """))
                .Run();

        Assert.Single(model.Identities);

        Assert.Equal(
            "Cliente",
            model.Identities.Single().Id);
    }
}