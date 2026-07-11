using X7.Knowledge;
using Xunit;

namespace X7.KnowledgeTests;

public sealed class KnowledgeModelTests
{
    [Fact]
    public void Deve_criar_modelo_vazio()
    {
        var model = new KnowledgeModel();

        Assert.Empty(model.Identities);
    }
}