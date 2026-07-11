using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class PropertyVisitor : CSharpSyntaxWalker
{
    private readonly TypeNode _type;

    public PropertyVisitor(TypeNode type)
    {
        _type = type;
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        var property = new PropertyNode
        {
            Id = $"{_type.Id}.{node.Identifier.Text}",

            TypeId = _type.Id,

            Name = node.Identifier.Text,

            Type = node.Type.ToString(),

            Accessibility = GetAccessibility(node),

            HasGetter = node.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false,

            HasSetter = node.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false,

            InitOnly = node.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.InitAccessorDeclaration)) ?? false
        };

        _type.Properties.Add(property);

        base.VisitPropertyDeclaration(node);
    }

    private static string GetAccessibility(PropertyDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PublicKeyword))
            return "public";

        if (node.Modifiers.Any(SyntaxKind.InternalKeyword))
            return "internal";

        if (node.Modifiers.Any(SyntaxKind.ProtectedKeyword) &&
            node.Modifiers.Any(SyntaxKind.InternalKeyword))
            return "protected internal";

        if (node.Modifiers.Any(SyntaxKind.ProtectedKeyword))
            return "protected";

        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
            return "private";

        return "private";
    }
}