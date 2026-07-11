using System.Text.Json;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Knowledge;

public sealed class GraphExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var semantic = new SemanticExportBuilder().Build(index);

        File.WriteAllText(
            Path.Combine(folder, "semantic.json"),
            JsonSerializer.Serialize(
                semantic,
                options));
    }
}