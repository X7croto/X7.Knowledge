namespace X7.Knowledge.Model;

/// <summary>Constituição §5.3 — nível em que o conhecimento foi obtido.</summary>
public enum AcquisitionLevel
{
    /// <summary>X — apenas o que a árvore sintática garante.</summary>
    Syntactic = 0,

    /// <summary>S — modelo semântico disponível.</summary>
    Semantic = 1
}

public static class AcquisitionLevelExtensions
{
    public static string ToToken(this AcquisitionLevel level)
        => level == AcquisitionLevel.Semantic ? "S" : "X";
}
