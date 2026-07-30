using System.Globalization;
using System.Text;

namespace X7.Knowledge.Serialization;

/// <summary>
/// Árvore de valores JSON com serialização canônica (D-06):
/// chaves ordenadas ordinalmente, LF, números invariantes, nulos omitidos.
/// Escrita à mão de propósito — o determinismo da saída não pode depender
/// de política interna de biblioteca.
/// </summary>
public abstract record CanonicalJson
{
    public sealed record JsonString(string Value) : CanonicalJson;

    public sealed record JsonNumber(long Value) : CanonicalJson;

    public sealed record JsonBoolean(bool Value) : CanonicalJson;

    public sealed record JsonArray(IReadOnlyList<CanonicalJson> Items) : CanonicalJson;

    public sealed record JsonObject(IReadOnlyDictionary<string, CanonicalJson> Members)
        : CanonicalJson;

    public static CanonicalJson Of(string value) => new JsonString(value);

    public static CanonicalJson Of(long value) => new JsonNumber(value);

    public static CanonicalJson Of(bool value) => new JsonBoolean(value);

    public static CanonicalJson Array(IEnumerable<CanonicalJson> items)
        => new JsonArray(items.ToArray());

    public static CanonicalJson Strings(IEnumerable<string> items)
        => new JsonArray(items.Select(Of).ToArray());

    /// <summary>Membros com valor nulo são omitidos, nunca escritos como null.</summary>
    public static CanonicalJson Object(params (string Key, CanonicalJson? Value)[] members)
    {
        var map = new SortedDictionary<string, CanonicalJson>(StringComparer.Ordinal);

        foreach (var (key, value) in members)
        {
            if (value is null)
                continue;

            map[key] = value;
        }

        return new JsonObject(map);
    }

    public string Serialize()
    {
        var builder = new StringBuilder();

        Write(this, builder, depth: 0);

        builder.Append('\n');

        return builder.ToString();
    }

    private static void Write(CanonicalJson node, StringBuilder builder, int depth)
    {
        switch (node)
        {
            case JsonString s:
                WriteString(s.Value, builder);
                break;

            case JsonNumber n:
                builder.Append(n.Value.ToString(CultureInfo.InvariantCulture));
                break;

            case JsonBoolean b:
                builder.Append(b.Value ? "true" : "false");
                break;

            case JsonArray a:
                WriteArray(a, builder, depth);
                break;

            case JsonObject o:
                WriteObject(o, builder, depth);
                break;

            default:
                throw new InvalidOperationException($"Nó JSON desconhecido: {node.GetType().Name}");
        }
    }

    private static void WriteArray(JsonArray array, StringBuilder builder, int depth)
    {
        if (array.Items.Count == 0)
        {
            builder.Append("[]");
            return;
        }

        builder.Append('[');

        for (var i = 0; i < array.Items.Count; i++)
        {
            if (i > 0)
                builder.Append(',');

            NewLine(builder, depth + 1);

            Write(array.Items[i], builder, depth + 1);
        }

        NewLine(builder, depth);
        builder.Append(']');
    }

    private static void WriteObject(JsonObject obj, StringBuilder builder, int depth)
    {
        if (obj.Members.Count == 0)
        {
            builder.Append("{}");
            return;
        }

        builder.Append('{');

        var first = true;

        // Ordenação ordinal garantida pelo SortedDictionary da fábrica;
        // reforçada aqui para tolerar mapas construídos de outra forma.
        foreach (var pair in obj.Members.OrderBy(m => m.Key, StringComparer.Ordinal))
        {
            if (!first)
                builder.Append(',');

            first = false;

            NewLine(builder, depth + 1);

            WriteString(pair.Key, builder);
            builder.Append(": ");

            Write(pair.Value, builder, depth + 1);
        }

        NewLine(builder, depth);
        builder.Append('}');
    }

    private static void NewLine(StringBuilder builder, int depth)
    {
        builder.Append('\n');
        builder.Append(' ', depth * 2);
    }

    private static void WriteString(string value, StringBuilder builder)
    {
        builder.Append('"');

        foreach (var c in value)
        {
            switch (c)
            {
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                case '\b': builder.Append("\\b"); break;
                case '\f': builder.Append("\\f"); break;
                default:
                    if (c < 0x20)
                    {
                        builder.Append("\\u")
                               .Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(c);
                    }
                    break;
            }
        }

        builder.Append('"');
    }
}
