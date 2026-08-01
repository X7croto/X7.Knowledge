namespace X7.Knowledge.Model;

/// <summary>
/// Vocabulários fechados dos kinds de membro (KNOWLEDGE_MODEL §6.1.4).
/// Valor fora do vocabulário é erro de compilação, como kind fora do
/// catálogo (IV-04).
/// </summary>
/// <remarks>
/// A acessibilidade de membro reusa <see cref="TypeVocabulary"/>: são as
/// mesmas seis formas da linguagem, e duplicá-las aqui criaria duas listas
/// que divergiriam em silêncio.
/// </remarks>
public static class MemberVocabulary
{
    public const string Method = "method";
    public const string Constructor = "constructor";
    public const string Property = "property";

    public const string Get = "get";
    public const string Set = "set";
    public const string Init = "init";

    private static readonly HashSet<string> KindCatalog = new(StringComparer.Ordinal)
    {
        Method, Constructor, Property
    };

    /// <summary>
    /// Apenas o que a declaração escreve. `async` fica de fora por ser
    /// detalhe de implementação e não superfície; `new` descreve ocultação e
    /// não é exposto pelo símbolo; `partial` pelo mesmo motivo já registrado
    /// em <see cref="TypeVocabulary"/>.
    /// </summary>
    private static readonly HashSet<string> ModifierCatalog = new(StringComparer.Ordinal)
    {
        "static", "abstract", "virtual", "override", "sealed",
        "readonly", "required", "extern"
    };

    private static readonly HashSet<string> AccessorCatalog = new(StringComparer.Ordinal)
    {
        Get, Set, Init
    };

    private static readonly HashSet<string> ParameterModifierCatalog = new(StringComparer.Ordinal)
    {
        "ref", "out", "in", "params", "ref-readonly"
    };

    public static bool IsKnownKind(string value) => KindCatalog.Contains(value);

    public static bool IsKnownModifier(string value) => ModifierCatalog.Contains(value);

    public static bool IsKnownAccessor(string value) => AccessorCatalog.Contains(value);

    public static bool IsKnownParameterModifier(string value)
        => ParameterModifierCatalog.Contains(value);

    public static IReadOnlyCollection<string> Kinds => KindCatalog;

    public static IReadOnlyCollection<string> Modifiers => ModifierCatalog;

    public static IReadOnlyCollection<string> Accessors => AccessorCatalog;

    public static IReadOnlyCollection<string> ParameterModifiers => ParameterModifierCatalog;
}
