using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class NamespaceVisitor : CSharpSyntaxWalker
{
    private readonly SourceFile _file;

    public NamespaceVisitor(SourceFile file)
    {
        _file = file;
    }

    public override void VisitNamespaceDeclaration(
        NamespaceDeclarationSyntax node)
    {
        _file.Namespace = node.Name.ToString();

        base.VisitNamespaceDeclaration(node);
    }

    public override void VisitFileScopedNamespaceDeclaration(
        FileScopedNamespaceDeclarationSyntax node)
    {
        _file.Namespace = node.Name.ToString();

        base.VisitFileScopedNamespaceDeclaration(node);
    }
}