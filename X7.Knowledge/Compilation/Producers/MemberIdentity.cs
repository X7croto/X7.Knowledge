using Microsoft.CodeAnalysis;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// Cálculo da identidade de um membro, em um único lugar
/// (KNOWLEDGE_MODEL §3.1).
/// </summary>
/// <remarks>
/// Mesma razão de <see cref="TypeIdentity"/>: duas implementações da mesma
/// identidade divergem em silêncio, e o modelo passa a ter dois membros onde
/// há um.
/// </remarks>
internal static class MemberIdentity
{
    /// <summary>
    /// Qualificação sem apelido de tipo especial: `System.String`, e não
    /// `string`.
    /// </summary>
    /// <remarks>
    /// Usado só na identidade. O apelido é convenção de escrita do C# e pode
    /// mudar de forma sem que o tipo mude; a identidade precisa do nome do
    /// tipo. Para exibição vale o contrário, e aí quem responde é
    /// <see cref="TypeIdentity.Display"/>.
    /// </remarks>
    private static readonly SymbolDisplayFormat Qualified = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    /// <summary>
    /// `{tipoQualificado}.{nome}({tiposDosParâmetros})` para método e
    /// construtor; sem parênteses para propriedade.
    /// </summary>
    /// <remarks>
    /// Os tipos de parâmetro entram na forma construída — `List&lt;string&gt;`
    /// e `List&lt;int&gt;` são sobrecargas distintas e precisam de identidades
    /// distintas. Isso não vale para `typeId` dentro de payload, que aponta a
    /// definição original (IV-13): a identidade do tipo é a declaração, a
    /// assinatura do membro é o uso.
    ///
    /// O nome vem de `MetadataName`, que já traz `.ctor` para construtor e a
    /// aridade do método genérico. Ausência de parênteses distingue
    /// propriedade de método sem parâmetros.
    /// </remarks>
    public static string Semantic(ISymbol member)
    {
        var container = TypeIdentity.Semantic(member.ContainingType);

        return member switch
        {
            IMethodSymbol method
                => $"{container}.{method.MetadataName}({Parameters(method.Parameters)})",

            // Indexador é propriedade com parâmetros: sobrecargas existem, e
            // sem os tipos duas viram uma. Colchete, e não parêntese, porque
            // é o que a declaração escreve — e porque parêntese o tornaria
            // indistinguível de um método (ADR-042).
            IPropertySymbol { IsIndexer: true } indexer
                => $"{container}.this[{Parameters(indexer.Parameters)}]",

            _ => $"{container}.{member.MetadataName}"
        };
    }

    private static string Parameters(IEnumerable<IParameterSymbol> parameters)
        => string.Join(",", parameters.Select(p => p.Type.ToDisplayString(Qualified)));
}
