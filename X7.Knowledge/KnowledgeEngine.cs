namespace X7.Knowledge;

public sealed class KnowledgeEngine
{
    private KnowledgeEngine()
    {
    }

    public static KnowledgeEngine Create()
    {
        return new KnowledgeEngine();
    }

    public KnowledgeSession Start()
    {
        return new KnowledgeSession();
    }
}