using System.Text;

namespace X7.Knowledge.Model;

/// <summary>
/// Dados de uma Observation: mapa ordenado de chave para valor.
/// v0 usa apenas valores textuais — todo o catálogo C01 cabe nessa forma.
/// Payload tipado entra quando algum kind exigir (EX-01).
/// </summary>
public sealed class ObservationPayload
{
    private readonly SortedDictionary<string, string> _values;

    private ObservationPayload(SortedDictionary<string, string> values)
        => _values = values;

    public static ObservationPayload Empty { get; } =
        new(new SortedDictionary<string, string>(StringComparer.Ordinal));

    public static ObservationPayload From(params (string Key, string? Value)[] entries)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, value) in entries)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            // Campo nulo é ausência; nunca escrito como "null".
            if (value is null)
                continue;

            values[key] = value;
        }

        return new ObservationPayload(values);
    }

    public IReadOnlyDictionary<string, string> Values => _values;

    public string? this[string key]
        => _values.TryGetValue(key, out var value) ? value : null;

    /// <summary>
    /// Forma canônica usada no cálculo de identidade.
    /// Chaves ordenadas ordinalmente; separadores escapados.
    /// </summary>
    public string ToCanonicalString()
    {
        var builder = new StringBuilder();

        foreach (var pair in _values)
        {
            if (builder.Length > 0)
                builder.Append(';');

            Escape(builder, pair.Key);
            builder.Append('=');
            Escape(builder, pair.Value);
        }

        return builder.ToString();
    }

    private static void Escape(StringBuilder builder, string value)
    {
        foreach (var c in value)
        {
            if (c is '\\' or ';' or '=')
                builder.Append('\\');

            builder.Append(c);
        }
    }
}
