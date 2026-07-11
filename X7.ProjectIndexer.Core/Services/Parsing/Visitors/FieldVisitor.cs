using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class FieldVisitor : CSharpSyntaxWalker
{
    private readonly TypeNode _type;

    public FieldVisitor(TypeNode type)
    {
        _type = type;
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (var variable in node.Declaration.Variables)
        {
            var field = new FieldNode
            {
                Id = $"{_type.Id}.{variable.Identifier.Text}",

                TypeId = _type.Id,

                Name = variable.Identifier.Text,

                Type = node.Declaration.Type.ToString(),

                Accessibility = GetAccessibility(node),

                Static = node.Modifiers.Any(SyntaxKind.StaticKeyword),

                Readonly = node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword),

                Const = node.Modifiers.Any(SyntaxKind.ConstKeyword)
            };

            _type.Fields.Add(field);
        }

        base.VisitFieldDeclaration(node);
    }

    private static string GetAccessibility(FieldDeclarationSyntax node)
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