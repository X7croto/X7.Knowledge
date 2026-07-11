using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Services.Knowledge.Query.Models;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Query.Builders;

public sealed class TypeIndexBuilder
{
    public IEnumerable<TypeIndex> Build(ProjectIndexOld index)
    {
        foreach (var type in index.Semantic.Types)
        {
            var model = new TypeIndex
            {
                Id = type.Id,
                Name = type.Name,
                Namespace = type.Namespace,
                Layer = type.Layer.ToString()
            };

            model.Methods.AddRange(
                type.Methods.Select(x => x.Name));

            model.Dependencies.AddRange(
                index.Semantic.Dependencies
                    .Where(x => x.Source == type)
                    .Select(x => x.Target.Name)
                    .Distinct());

            model.Dependents.AddRange(
                index.Semantic.Dependencies
                    .Where(x => x.Target == type)
                    .Select(x => x.Source.Name)
                    .Distinct());

            if (type.BaseType != null)
                model.Inherits.Add(type.BaseType);

            model.Implements.AddRange(type.Interfaces);

            yield return model;
        }
    }
}