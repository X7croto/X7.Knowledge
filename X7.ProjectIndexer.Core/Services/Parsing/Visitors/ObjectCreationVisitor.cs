using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class ObjectCreationVisitor : CSharpSyntaxWalker
{
    private readonly BlockNode _block;

    public ObjectCreationVisitor(BlockNode block)
    {
        _block = block;
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        _block.ObjectCreations.Add(new ObjectCreationNode
        {
            Type = node.Type.ToString(),
            Line = node
                .GetLocation()
                .GetLineSpan()
                .StartLinePosition.Line + 1,
            Expression = node.ToString()
        });

        base.VisitObjectCreationExpression(node);
    }
}