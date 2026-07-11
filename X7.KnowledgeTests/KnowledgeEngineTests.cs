using X7.Knowledge;
using Xunit;

namespace X7.KnowledgeTests;

public sealed class KnowledgeEngineTests
{
    [Fact]
    public void Deve_criar_uma_sessao()
    {
        var session =
            KnowledgeEngine
                .Create()
                .Start();

        Assert.NotNull(session);
    }
}