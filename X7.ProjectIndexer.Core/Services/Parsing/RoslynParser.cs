using Microsoft.CodeAnalysis.CSharp;
using X7.ProjectIndexer.Core.Services.Parsing;
using X7.ProjectIndexer.Core.Services.Parsing.Visitors;

public sealed class RoslynParser : IParser
{
    public void Parse(SourceFile file)
    {
        if (!file.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            return;

        var text = File.ReadAllText(file.Path);

        var tree = CSharpSyntaxTree.ParseText(text);

        var root = tree.GetCompilationUnitRoot();

        new CompilationUnitVisitor(file).Visit(root);
    }
}