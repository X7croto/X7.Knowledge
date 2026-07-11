using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Relations;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class BlockVisitor : CSharpSyntaxWalker
{
    private readonly BlockNode _block;

    public BlockVisitor(BlockNode block)
    {
        _block = block;
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        var type = node.Declaration.Type.ToString();

        foreach (var variable in node.Declaration.Variables)
        {
            _block.LocalVariables.Add(new LocalVariableNode
            {
                Name = variable.Identifier.Text,
                Type = type
            });
        }

        base.VisitLocalDeclarationStatement(node);
    }

    public override void VisitInvocationExpression(
        InvocationExpressionSyntax node)
    {
        string? target = null;
        string method;

        if (node.Expression is MemberAccessExpressionSyntax member)
        {
            target = member.Expression.ToString();

            method = member.Name.Identifier.Text;
        }
        else
        {
            method = node.Expression.ToString();
        }

        _block.Invocations.Add(new InvocationNode
        {
            Name = method,

            Target = target,

            Expression = node.ToString(),

            Line = node.GetLocation()
                       .GetLineSpan()
                       .StartLinePosition.Line + 1
        });

        base.VisitInvocationExpression(node);
    }
    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        _block.Returns.Add(new ReturnNode
        {
            Expression = node.Expression?.ToString(),
            Line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1
        });

        base.VisitReturnStatement(node);
    }
    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        _block.ObjectCreations.Add(new ObjectCreationNode
        {
            Type = node.Type.ToString(),

            Expression = node.ToString(),

            Line = node.GetLocation()
                       .GetLineSpan()
                       .StartLinePosition
                       .Line + 1
        });

        base.VisitObjectCreationExpression(node);
    }
    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        _block.MemberAccesses.Add(new MemberAccessNode
        {
            Expression = node.ToString(),

            Target = node.Expression.ToString(),

            Member = node.Name.Identifier.Text,

            Line = node.GetLocation()
                       .GetLineSpan()
                       .StartLinePosition.Line + 1
        });

        base.VisitMemberAccessExpression(node);
    }
}