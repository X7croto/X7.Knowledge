namespace X7.Knowledge.Model;

/// <summary>
/// Vocabulários fechados dos kinds de estrutura de tipo (KNOWLEDGE_MODEL
/// §6.1.3.b). Valor fora do vocabulário é erro de compilação, como kind fora
/// do catálogo (IV-04).
/// </summary>
/// <remarks>
/// O vocabulário espelha exatamente as formas declaráveis em C#. `record` é
/// o `record class`; `record-struct` tem valor próprio porque é o que está
/// escrito na declaração — classificá-lo como `struct` seria interpretar.
/// </remarks>
public static class TypeVocabulary
{
    public const string Class = "class";
    public const string Interface = "interface";
    public const string Record = "record";
    public const string RecordStruct = "record-struct";
    public const string Struct = "struct";
    public const string Enum = "enum";
    public const string Delegate = "delegate";

    public const string Public = "public";
    public const string Internal = "internal";
    public const string Protected = "protected";
    public const string Private = "private";
    public const string ProtectedInternal = "protected-internal";
    public const string PrivateProtected = "private-protected";

    private static readonly HashSet<string> KindCatalog = new(StringComparer.Ordinal)
    {
        Class, Interface, Record, RecordStruct, Struct, Enum, Delegate
    };

    private static readonly HashSet<string> AccessibilityCatalog = new(StringComparer.Ordinal)
    {
        Public, Internal, Protected, Private, ProtectedInternal, PrivateProtected
    };

    /// <summary>
    /// Apenas o que altera a natureza do tipo declarado. `partial` não
    /// pertence a este vocabulário: não existe no símbolo e é tratado como
    /// Inference (`type.is-partial`). `new` e os de acessibilidade também
    /// ficam de fora — o primeiro descreve ocultação, os últimos têm kind
    /// próprio.
    /// </summary>
    private static readonly HashSet<string> ModifierCatalog = new(StringComparer.Ordinal)
    {
        "abstract", "sealed", "static", "readonly", "ref", "unsafe"
    };

    public static bool IsKnownKind(string value) => KindCatalog.Contains(value);

    public static bool IsKnownAccessibility(string value) => AccessibilityCatalog.Contains(value);

    public static bool IsKnownModifier(string value) => ModifierCatalog.Contains(value);

    public static IReadOnlyCollection<string> Kinds => KindCatalog;

    public static IReadOnlyCollection<string> Accessibilities => AccessibilityCatalog;

    public static IReadOnlyCollection<string> Modifiers => ModifierCatalog;
}
