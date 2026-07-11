using System.Text;

namespace X7.ProjectIndexer.Knowledge;

public sealed class MarkdownWriter
{
    private readonly StringBuilder _builder = new();

    public void H1(string text)
    {
        _builder.AppendLine($"# {text}");
        _builder.AppendLine();
    }

    public void H2(string text)
    {
        _builder.AppendLine($"## {text}");
        _builder.AppendLine();
    }

    public void H3(string text)
    {
        _builder.AppendLine($"### {text}");
        _builder.AppendLine();
    }

    public void Line(string text = "")
    {
        _builder.AppendLine(text);
    }

    public void Bullet(string text)
    {
        _builder.AppendLine($"- {text}");
    }

    public void Code(string code)
    {
        _builder.AppendLine("```");

        _builder.AppendLine(code);

        _builder.AppendLine("```");
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}