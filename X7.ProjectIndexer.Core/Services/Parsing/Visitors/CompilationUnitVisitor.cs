using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class CompilationUnitVisitor
{
    private readonly SourceFile _file;

    public CompilationUnitVisitor(SourceFile file)
    {
        _file = file;
    }

    public void Visit(CompilationUnitSyntax root)
    {
        new NamespaceVisitor(_file).Visit(root);

        new UsingVisitor(_file).Visit(root);

        new TypeVisitor(_file).Visit(root);
    }
}