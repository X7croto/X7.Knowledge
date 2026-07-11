using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class IdentifierVisitor : CSharpSyntaxWalker
{
    private readonly BlockNode _block;

    public IdentifierVisitor(BlockNode block)
    {
        _block = block;
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        _block.Identifiers.Add(new IdentifierNode
        {
            Name = node.Identifier.Text,

            Line = node.GetLocation()
                       .GetLineSpan()
                       .StartLinePosition.Line + 1
        });

        base.VisitIdentifierName(node);
    }
}