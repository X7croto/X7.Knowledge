using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Parsing;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class UsingVisitor : CSharpSyntaxWalker
{
    private readonly SourceFile _file;

    public UsingVisitor(SourceFile file)
    {
        _file = file;
    }

    public override void VisitUsingDirective(
        UsingDirectiveSyntax node)
    {
        _file.Usings.Add(
            new UsingDirective
            {
                Namespace = node.Name?.ToString() ?? "",
                Alias = node.Alias?.Name.ToString(),
                IsStatic = node.StaticKeyword != default,
                IsGlobal = node.GlobalKeyword != default
            });

        base.VisitUsingDirective(node);
    }
}