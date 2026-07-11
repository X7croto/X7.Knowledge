using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Relations;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class ReturnVisitor : CSharpSyntaxWalker
{
    private readonly BlockNode _block;

    public ReturnVisitor(BlockNode block)
    {
        _block = block;
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        _block.Returns.Add(new ReturnNode
        {
            Expression = node.Expression?.ToString(),
            Line = node.SyntaxTree.GetLineSpan(node.Span)
                .StartLinePosition.Line + 1
        });

        base.VisitReturnStatement(node);
    }
}