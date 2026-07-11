using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Knowledge;

public sealed class MethodExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        foreach (var method in index.Semantic.Methods)
        {
            var md = new MarkdownWriter();

            md.H1(method.Name);

            md.Line($"Type: {method.DeclaringType?.Name}");
            md.Line($"Return: {method.ReturnType}");
            md.Line();

            md.H2("Parameters");

            foreach (var parameter in method.Parameters)
            {
                md.Bullet($"{parameter.Type} {parameter.Name}");
            }

            md.Line();

            md.H2("Calls");

            foreach (var callee in method.Callees.OrderBy(x => x.Name))
            {
                md.Bullet($"{callee.DeclaringType?.Name}.{callee.Name}");
            }

            md.Line();

            md.H2("Called By");

            foreach (var caller in method.Callers.OrderBy(x => x.Name))
            {
                md.Bullet($"{caller.DeclaringType?.Name}.{caller.Name}");
            }

            var file = Path.Combine(
                folder,
                "methods",
                $"{method.DeclaringType?.Name}.{method.Name}.md");

            File.WriteAllText(file, md.ToString());
        }
    }
}