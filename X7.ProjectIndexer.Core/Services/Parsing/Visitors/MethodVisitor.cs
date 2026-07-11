using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class MethodVisitor : CSharpSyntaxWalker
{
    private readonly TypeNode _type;

    public MethodVisitor(TypeNode type)
    {
        _type = type;
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var method = new MethodNode
        {
            Id = $"{_type.Id}.{node.Identifier.Text}",
            TypeId = _type.Id,
            Name = node.Identifier.Text,
            ReturnType = node.ReturnType.ToString(),
            Accessibility = GetAccessibility(node),
            Static = node.Modifiers.Any(SyntaxKind.StaticKeyword),
            Virtual = node.Modifiers.Any(SyntaxKind.VirtualKeyword),
            Override = node.Modifiers.Any(SyntaxKind.OverrideKeyword),
            Abstract = node.Modifiers.Any(SyntaxKind.AbstractKeyword),
            Async = node.Modifiers.Any(SyntaxKind.AsyncKeyword)
        };

        foreach (var attributeList in node.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                method.Attributes.Add(attribute.Name.ToString());
            }
        }

        new ParameterVisitor(method).Visit(node.ParameterList);

        new BlockVisitor(method.Body).Visit(node.Body);
        new BlockVisitor(method.Body).Visit(node.ExpressionBody);

        new AssignmentVisitor(method.Body).Visit(node.Body);
        new AssignmentVisitor(method.Body).Visit(node.ExpressionBody);

        new ReturnVisitor(method.Body).Visit(node.Body);
        new ReturnVisitor(method.Body).Visit(node.ExpressionBody);

        new ObjectCreationVisitor(method.Body).Visit(node.Body);
        new ObjectCreationVisitor(method.Body).Visit(node.ExpressionBody);

        new MemberAccessVisitor(method.Body).Visit(node.Body);
        new MemberAccessVisitor(method.Body).Visit(node.ExpressionBody);

        new IdentifierVisitor(method.Body).Visit(node.Body);
        new IdentifierVisitor(method.Body).Visit(node.ExpressionBody); 
        
        var bodyVisitor = new BlockVisitor(method.Body);

        bodyVisitor.Visit(node.Body);

        bodyVisitor.Visit(node.ExpressionBody);

        _type.Methods.Add(method);

        base.VisitMethodDeclaration(node);
    }

    private static string GetAccessibility(MethodDeclarationSyntax node)
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