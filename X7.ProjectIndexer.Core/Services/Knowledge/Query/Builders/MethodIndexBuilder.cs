using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Builders;

public sealed class MethodIndexBuilder
{
    public IEnumerable<MethodIndex> Build(ProjectIndexOld index)
    {
        foreach (var method in index.Semantic.Methods)
        {
            var model = new MethodIndex
            {
                Id = method.Id,
                Name = method.Name,
                Type = method.DeclaringType?.Name ?? "",
                EntryPoint = method.IsEntryPoint,
                Recursive = method.Recursive,
                DeadCode = method.IsDeadCode
            };

            model.Calls.AddRange(
                method.Callees.Select(x =>
                    $"{x.DeclaringType?.Name}.{x.Name}"));

            model.CalledBy.AddRange(
                method.Callers.Select(x =>
                    $"{x.DeclaringType?.Name}.{x.Name}"));

            yield return model;
        }
    }
}