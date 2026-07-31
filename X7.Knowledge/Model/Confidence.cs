namespace X7.Knowledge.Model;

/// <summary>
/// Atributo obrigatório de toda Inference (Constituição §3.3, PR-11).
/// O compilador nunca apresenta regularidade estatística como verdade absoluta.
/// </summary>
public enum Confidence
{
    /// <summary>Regra exata, sem exceções. Não admite frequência.</summary>
    Asserted = 0,

    /// <summary>Regularidade estatística. Exige frequência declarada.</summary>
    Observed = 1
}

public static class ConfidenceExtensions
{
    public static string ToToken(this Confidence confidence)
        => confidence == Confidence.Asserted ? "Asserted" : "Observed";
}
