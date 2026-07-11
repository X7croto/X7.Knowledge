using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;

namespace X7.ProjectIndexer.Core.Services.Knowledge;

public sealed class ModuleCompiler
{
    public void Build(ProjectIndexOld index)
    {
        index.Knowledge.Architecture.Modules.Clear();

        foreach (var group in index.Semantic.Types
                     .GroupBy(t => t.Namespace)
                     .OrderBy(g => g.Key))
        {
            var module = new ModuleModel
            {
                Name = group.Key
            };

            foreach (var type in group)
                module.Types.Add(type.Id);

            index.Knowledge.Architecture.Modules.Add(module);
        }
    }
}