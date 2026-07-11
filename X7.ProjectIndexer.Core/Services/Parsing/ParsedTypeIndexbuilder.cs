using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing;

public sealed class ParsedTypeIndexBuilder
{
    public void Build(ProjectIndexOld index)
    {
        index.ParsedTypesByFullName.Clear();
        index.ParsedTypesByName.Clear();

        foreach (var project in index.Projects)
            foreach (var file in project.Files)
                foreach (var type in file.Types)
                {
                    index.ParsedTypesByFullName[type.Id] = type;

                    if (!index.ParsedTypesByName.TryGetValue(type.Name, out var list))
                    {
                        list = [];
                        index.ParsedTypesByName[type.Name] = list;
                    }

                    list.Add(type);
                }
    }
}