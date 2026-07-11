using System.Text.Json;
using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Export;

public sealed class KnowledgeIndexExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public void Export(ProjectIndexOld index, string folder)
    {
        Directory.CreateDirectory(folder);

        ExportTypes(index.QueryIndex, folder);

        ExportMethods(index.QueryIndex, folder);

        ExportNamespaces(index.QueryIndex, folder);

        ExportFeatures(index.QueryIndex, folder);

        ExportOverview(index, folder);
    }

    private static void ExportTypes(
        KnowledgeIndex knowledge,
        string folder)
    {
        File.WriteAllText(
            Path.Combine(folder, "types.json"),
            JsonSerializer.Serialize(
                knowledge.Types,
                Options));
    }

    private static void ExportMethods(
        KnowledgeIndex knowledge,
        string folder)
    {
        File.WriteAllText(
            Path.Combine(folder, "methods.json"),
            JsonSerializer.Serialize(
                knowledge.Methods,
                Options));
    }

    private static void ExportNamespaces(
        KnowledgeIndex knowledge,
        string folder)
    {
        File.WriteAllText(
            Path.Combine(folder, "namespaces.json"),
            JsonSerializer.Serialize(
                knowledge.Namespaces,
                Options));
    }

    private static void ExportFeatures(
        KnowledgeIndex knowledge,
        string folder)
    {
        File.WriteAllText(
            Path.Combine(folder, "features.json"),
            JsonSerializer.Serialize(
                knowledge.Features,
                Options));
    }

    private static void ExportOverview(
        ProjectIndexOld index,
        string folder)
    {
        var overview = new
        {
            Projects = index.Semantic.Projects.Count,
            Types = index.Semantic.Types.Count,
            Methods = index.Semantic.Methods.Count,
            Properties = index.Semantic.Properties.Count,
            Fields = index.Semantic.Fields.Count,
            Dependencies = index.Semantic.Dependencies.Count,
            Calls = index.Semantic.Calls.Count,
            Namespaces = index.QueryIndex.Namespaces.Count,
            Features = index.QueryIndex.Features.Count
        };

        File.WriteAllText(
            Path.Combine(folder, "overview.json"),
            JsonSerializer.Serialize(
                overview,
                Options));
    }
}