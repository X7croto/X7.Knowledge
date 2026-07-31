namespace X7.Knowledge.Model;

/// <summary>
/// Ocorrências conformes sobre total. Obrigatória quando a Confidence é
/// Observed; proibida quando é Asserted.
/// </summary>
public sealed record Frequency
{
    public Frequency(int matching, int total)
    {
        if (total <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(total), total, "Frequência exige total maior que zero.");

        if (matching < 0 || matching > total)
            throw new ArgumentOutOfRangeException(
                nameof(matching), matching, $"Conformes fora do intervalo [0, {total}].");

        Matching = matching;
        Total = total;
    }

    public int Matching { get; }

    public int Total { get; }

    /// <summary>Em milésimos. Inteiro de propósito: double na saída canônica é risco.</summary>
    public int RatePerMille => (int)Math.Round(Matching * 1000.0 / Total);

    public override string ToString() => $"{Matching}/{Total}";
}
