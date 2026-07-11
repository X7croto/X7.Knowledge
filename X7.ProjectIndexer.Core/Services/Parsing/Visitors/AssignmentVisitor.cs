using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Relations;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class AssignmentVisitor : CSharpSyntaxWalker
{
    private readonly BlockNode _block;

    public AssignmentVisitor(BlockNode block)
    {
        _block = block;
    }

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        _block.Assignments.Add(new AssignmentNode
        {
            Left = node.Left.ToString(),
            Right = node.Right.ToString(),
            Line = node.SyntaxTree.GetLineSpan(node.Span)
                .StartLinePosition.Line + 1
        });

        base.VisitAssignmentExpression(node);
    }
}