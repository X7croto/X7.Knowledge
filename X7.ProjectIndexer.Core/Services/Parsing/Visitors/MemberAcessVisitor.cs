using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class MemberAccessVisitor : CSharpSyntaxWalker
{
    private readonly BlockNode _block;

    public MemberAccessVisitor(BlockNode block)
    {
        _block = block;
    }

    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        _block.MemberAccesses.Add(new MemberAccessNode
        {
            Expression = node.ToString(),

            Target = node.Expression.ToString(),

            Member = node.Name.Identifier.Text,

            Line = node.SyntaxTree
                .GetLineSpan(node.Span)
                .StartLinePosition.Line + 1
        });

        base.VisitMemberAccessExpression(node);
    }
}