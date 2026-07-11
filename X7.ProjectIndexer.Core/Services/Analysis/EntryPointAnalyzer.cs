using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Analysis;

public sealed class EntryPointAnalyzer : IAnalyzer
{
    public void Analyze(ProjectIndexOld index)
    {
        foreach (var method in index.Semantic.Methods)
        {
            if (method.Name == "Main")
            {
                method.IsEntryPoint = true;
                continue;
            }

            if (method.Name == "Execute")
            {
                method.IsEntryPoint = true;
                continue;
            }

            if (method.Name == "OnStartup")
            {
                method.IsEntryPoint = true;
                continue;
            }

            if (method.Name == "OnShutdown")
            {
                method.IsEntryPoint = true;
                continue;
            }
        }
    }
}