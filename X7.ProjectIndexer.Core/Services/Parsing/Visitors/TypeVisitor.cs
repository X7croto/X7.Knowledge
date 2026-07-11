using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Parsing;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class TypeVisitor : CSharpSyntaxWalker
{
    private readonly SourceFile _file;

    public TypeVisitor(SourceFile file)
    {
        _file = file;
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        _file.Usings.Add(new UsingDirective
        {
            Namespace = node.Name?.ToString() ?? "",
            Alias = node.Alias?.Name.ToString(),
            IsStatic = node.StaticKeyword != default,
            IsGlobal = node.GlobalKeyword != default
        });

        base.VisitUsingDirective(node);
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        _file.Namespace = node.Name.ToString();

        base.VisitNamespaceDeclaration(node);
    }

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        _file.Namespace = node.Name.ToString();

        base.VisitFileScopedNamespaceDeclaration(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        VisitType(node, "class");
        base.VisitClassDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        VisitType(node, "struct");
        base.VisitStructDeclaration(node);
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        VisitType(node, "interface");
        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        VisitType(node, "record");
        base.VisitRecordDeclaration(node);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        VisitEnum(node);
        base.VisitEnumDeclaration(node);
    }

    private void VisitType(TypeDeclarationSyntax declaration, string kind)
    {
        var namespaceName = _file.Namespace ?? "";

        var typeNode = new TypeNode
        {
            Id = $"{namespaceName}.{declaration.Identifier.Text}",
            Name = declaration.Identifier.Text,
            Namespace = namespaceName,
            Kind = kind,
            Accessibility = GetAccessibility(declaration),
            Partial = declaration.Modifiers.Any(SyntaxKind.PartialKeyword),
            Static = declaration.Modifiers.Any(SyntaxKind.StaticKeyword),
            Abstract = declaration.Modifiers.Any(SyntaxKind.AbstractKeyword),
            Record = kind == "record",
            BaseType = declaration.BaseList?
                .Types
                .FirstOrDefault()?
                .Type
                .ToString()
        };

        foreach (var attribute in declaration.AttributeLists)
        {
            foreach (var a in attribute.Attributes)
                typeNode.Attributes.Add(a.Name.ToString());
        }

        if (declaration.BaseList is not null)
        {
            foreach (var baseType in declaration.BaseList.Types.Skip(1))
                typeNode.Interfaces.Add(baseType.Type.ToString());
        }

        new MethodVisitor(typeNode).Visit(declaration);
        new PropertyVisitor(typeNode).Visit(declaration);
        new FieldVisitor(typeNode).Visit(declaration);

        _file.Types.Add(typeNode);
    }

    private void VisitEnum(EnumDeclarationSyntax declaration)
    {
        var namespaceName = _file.Namespace ?? "";

        var typeNode = new TypeNode
        {
            Id = $"{namespaceName}.{declaration.Identifier.Text}",
            Name = declaration.Identifier.Text,
            Namespace = namespaceName,
            Kind = "enum",
            Accessibility = GetAccessibility(declaration),
            Partial = false,
            Static = false,
            Abstract = false,
            Record = false
        };

        foreach (var attribute in declaration.AttributeLists)
        {
            foreach (var a in declaration.AttributeLists.SelectMany(x => x.Attributes))
                typeNode.Attributes.Add(a.Name.ToString());
        }

        _file.Types.Add(typeNode);
    }

    private static string GetAccessibility(MemberDeclarationSyntax declaration)
    {
        var modifiers = declaration.GetModifiers();

        if (modifiers.Any(SyntaxKind.PublicKeyword))
            return "public";

        if (modifiers.Any(SyntaxKind.InternalKeyword))
            return "internal";

        if (modifiers.Any(SyntaxKind.ProtectedKeyword) &&
            modifiers.Any(SyntaxKind.InternalKeyword))
            return "protected internal";

        if (modifiers.Any(SyntaxKind.ProtectedKeyword))
            return "protected";

        if (modifiers.Any(SyntaxKind.PrivateKeyword))
            return "private";

        return "private";
    }
}

internal static class SyntaxExtensions
{
    public static SyntaxTokenList GetModifiers(this MemberDeclarationSyntax member)
    {
        return member switch
        {
            BaseTypeDeclarationSyntax t => t.Modifiers,
            BaseMethodDeclarationSyntax m => m.Modifiers,
            BasePropertyDeclarationSyntax p => p.Modifiers,
            BaseFieldDeclarationSyntax f => f.Modifiers,
            _ => default
        };
    }
}