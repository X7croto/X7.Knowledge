using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Query.Export;

namespace X7.ProjectIndexer.Knowledge;

public sealed class KnowledgeExporter : IKnowledgeExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        Directory.CreateDirectory(folder);

        Directory.CreateDirectory(Path.Combine(folder, "types"));
        Directory.CreateDirectory(Path.Combine(folder, "methods"));
        Directory.CreateDirectory(Path.Combine(folder, "projects"));
        Directory.CreateDirectory(Path.Combine(folder, "namespaces"));
        Directory.CreateDirectory(Path.Combine(folder, "reports"));

        new SolutionExporter().Export(index, folder);

        new ArchitectureExporter().Export(index, folder);

        new ProjectExporter().Export(index, folder);

        new NamespaceExporter().Export(index, folder);
        
        new MethodExporter().Export(index, folder);

        new TypeExporter().Export(index, folder);

        new GraphExporter().Export(index, folder);

        new KnowledgeGenerationReport().Export(index, folder);

        //para IAs
        new ClaudeExporter().Export(index, folder);

        new KnowledgeIndexExporter().Export(index,Path.Combine(folder, "index"));
    }
}