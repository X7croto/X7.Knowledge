using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// Cálculo da identidade de um tipo, em um único lugar.
/// </summary>
/// <remarks>
/// Existia uma cópia em cada Producer que precisava nomear tipos. Duas
/// implementações da mesma identidade divergem em silêncio: o modelo passa a
/// ter dois tipos onde há um, e IV-13 só acusa quando a referência já foi
/// escrita. Aqui há uma implementação por nível de aquisição, e nenhuma
/// duplicada.
/// </remarks>
internal static class TypeIdentity
{
    /// <summary>
    /// Como o tipo é identificado no modelo: sempre a definição original.
    /// `IFoo&lt;List&lt;string&gt;&gt;` e `IFoo&lt;int&gt;` são usos do mesmo
    /// tipo declarado `IFoo&lt;T&gt;`, e é ele que existe como identidade.
    /// </summary>
    public static string Semantic(INamedTypeSymbol type)
        => Qualify(type.OriginalDefinition);

    /// <summary>Como o tipo aparece no código, com argumentos genéricos.</summary>
    /// <remarks>
    /// Aceita qualquer tipo, e não apenas o nomeado: o C05 precisa exibir
    /// parâmetro de tipo (`T`), arranjo e ponteiro, que não são
    /// <see cref="INamedTypeSymbol"/>. Um só caminho de exibição evita que
    /// dois lugares formatem o mesmo tipo de formas diferentes.
    /// </remarks>
    public static string Display(ITypeSymbol type)
        => Qualify(type);

    private static string Qualify(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
               .Replace("global::", string.Empty, StringComparison.Ordinal);

    /// <summary>
    /// Nível X: o nome é montado a partir dos ancestrais sintáticos. Nada é
    /// resolvido — é o limite declarado do nível, não uma aproximação.
    /// </summary>
    public static SyntaxName Syntactic(SyntaxNode node)
    {
        var name = node switch
        {
            BaseTypeDeclarationSyntax t => t.Identifier.ValueText,
            DelegateDeclarationSyntax d => d.Identifier.ValueText,
            _ => throw new ArgumentException($"Nó não declara tipo: {node.Kind()}", nameof(node))
        };

        // Namespaces e tipos aninhados são acumulados em separado: o namespace
        // do tipo é só a parte de namespace.
        var namespaces = new List<string>();
        var outerTypes = new List<string>();

        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            switch (current)
            {
                case BaseNamespaceDeclarationSyntax ns:
                    namespaces.Insert(0, ns.Name.ToString());
                    break;

                case BaseTypeDeclarationSyntax outer:
                    outerTypes.Insert(0, outer.Identifier.ValueText);
                    break;
            }
        }

        var namespaceName = string.Join('.', namespaces);

        var qualified = string.Join('.', outerTypes.Append(name));

        return new SyntaxName
        {
            Name = name,
            Namespace = namespaceName,
            MetadataName = namespaceName.Length == 0
                ? qualified
                : $"{namespaceName}.{qualified}"
        };
    }

    /// <summary>Tipo contentor imediato, quando o nó é uma declaração aninhada.</summary>
    public static BaseTypeDeclarationSyntax? ContainerOf(SyntaxNode node)
        => node.Parent as BaseTypeDeclarationSyntax;

    internal sealed record SyntaxName
    {
        public required string Name { get; init; }

        public required string Namespace { get; init; }

        public required string MetadataName { get; init; }
    }
}
