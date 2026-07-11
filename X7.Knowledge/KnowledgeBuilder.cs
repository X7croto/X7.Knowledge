namespace X7.Knowledge;

internal sealed class KnowledgeBuilder
{
    private readonly KnowledgeModel _model = new();

    public Identity AddIdentity(string id)
    {
        return _model.AddIdentity(
            Identity.Create(id));
    }

    internal KnowledgeModel Build()
    {
        return _model;
    }
}