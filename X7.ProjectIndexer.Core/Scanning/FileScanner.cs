using System.Reflection;
using X7.ProjectIndexer.Core.Contracts;
using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Scanning;

public sealed class FileScanner : IFileScanner
{
    public ProjectIndexOld Scan(string root)
    {
        Console.WriteLine("SCANNING...");

        var index = new ProjectIndexOld
        {
            RootPath = root
        };

        foreach (var csproj in Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories))
        {
            var project = new ProjectNode
            {
                Name = Path.GetFileNameWithoutExtension(csproj),
                ProjectFile = csproj
            };

            var projectDirectory = Path.GetDirectoryName(csproj)!;

            foreach (var file in Directory.EnumerateFiles(projectDirectory, "*.*", SearchOption.AllDirectories))
            {
                if (file.Contains(@"\bin\") || file.Contains(@"\obj\"))
                    continue;

                var extension = Path.GetExtension(file);

                if (extension != ".cs" &&
                    extension != ".csproj" &&
                    extension != ".md" &&
                    extension != ".xaml")
                    continue;

                project.Files.Add(new SourceFile
                {
                    Path = file,
                    RelativePath = Path.GetRelativePath(projectDirectory, file)
                });
            }

            index.Projects.Add(project);
        }

        return index;
    }
}