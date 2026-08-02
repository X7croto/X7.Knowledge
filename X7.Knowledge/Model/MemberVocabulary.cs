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
    public const string Field = "field";
    public const string Event = "event";
    public const string Operator = "operator";
    public const string Indexer = "indexer";

    public const string KeywordConstraint = "keyword";
    public const string TypeConstraint = "type";
    public const string TypeParameterConstraint = "type-parameter";

    public const string Get = "get";
    public const string Set = "set";
    public const string Init = "init";
    public const string Add = "add";
    public const string Remove = "remove";

    /// <summary>
    /// Construtor estático não tem valor próprio: a declaração escreve
    /// `static X()`, e isso é `constructor` com o modificador `static`
    /// (ADR-042). Implementação explícita de interface também não: continua
    /// sendo método, propriedade ou evento, e o que a distingue é
    /// `member.explicit-interface`.
    /// </summary>
    private static readonly HashSet<string> KindCatalog = new(StringComparer.Ordinal)
    {
        Method, Constructor, Property, Field, Event, Operator, Indexer
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
        "readonly", "required", "extern", "const", "volatile"
    };

    private static readonly HashSet<string> AccessorCatalog = new(StringComparer.Ordinal)
    {
        Get, Set, Init, Add, Remove
    };

    private static readonly HashSet<string> ParameterModifierCatalog = new(StringComparer.Ordinal)
    {
        "ref", "out", "in", "params", "ref-readonly"
    };

    /// <summary>
    /// Distingue `T : U` de `T : class` sem depender da ausência de `typeId`.
    /// Ausência como discriminante é o que a ADR-039 §5 já rejeitou uma vez.
    /// </summary>
    private static readonly HashSet<string> ConstraintFormCatalog = new(StringComparer.Ordinal)
    {
        KeywordConstraint, TypeConstraint, TypeParameterConstraint
    };

    public static bool IsKnownKind(string value) => KindCatalog.Contains(value);

    public static bool IsKnownModifier(string value) => ModifierCatalog.Contains(value);

    public static bool IsKnownAccessor(string value) => AccessorCatalog.Contains(value);

    public static bool IsKnownParameterModifier(string value)
        => ParameterModifierCatalog.Contains(value);

    public static bool IsKnownConstraintForm(string value)
        => ConstraintFormCatalog.Contains(value);

    public static IReadOnlyCollection<string> Kinds => KindCatalog;

    public static IReadOnlyCollection<string> Modifiers => ModifierCatalog;

    public static IReadOnlyCollection<string> Accessors => AccessorCatalog;

    public static IReadOnlyCollection<string> ParameterModifiers => ParameterModifierCatalog;

    public static IReadOnlyCollection<string> ConstraintForms => ConstraintFormCatalog;
}
