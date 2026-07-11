using X7.ProjectIndexer.Core.Models;

public sealed class FileBindingContext
{
    public ProjectIndexOld Index { get; }

    public SourceFile File { get; }

    public Dictionary<string, string> TypeAliases { get; }

    public List<string> ImportedNamespaces { get; }

    public FileBindingContext(
        ProjectIndexOld index,
        SourceFile file)
    {
        Index = index;
        File = file;

        ImportedNamespaces =
            file.Usings
                .Where(x => !x.IsStatic)
                .Select(x => x.Namespace)
                .ToList();

        TypeAliases =
            file.Usings
                .Where(x => x.Alias is not null)
                .ToDictionary(
                    x => x.Alias!,
                    x => x.Namespace);
    }
}