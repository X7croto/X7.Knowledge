using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class ParameterVisitor : CSharpSyntaxWalker
{
    private readonly MethodNode _method;

    public ParameterVisitor(MethodNode method)
    {
        _method = method;
    }

    public override void VisitParameter(ParameterSyntax node)
    {
        var parameter = new ParameterNode
        {
            Name = node.Identifier.Text,
            Type = node.Type?.ToString() ?? ""
        };

        parameter.Ref = node.Modifiers.Any(SyntaxKind.RefKeyword);
        parameter.Out = node.Modifiers.Any(SyntaxKind.OutKeyword);
        parameter.Params = node.Modifiers.Any(SyntaxKind.ParamsKeyword);
        parameter.Optional = node.Default != null;

        _method.Parameters.Add(parameter);

        base.VisitParameter(node);
    }
}