using System.Xml.Linq;

namespace X7.Knowledge.Acquisition;

/// <summary>Leitor do formato .slnx (solução em XML).</summary>
internal static class SlnxReader
{
    public static SolutionFile Read(string solutionPath)
    {
        var root = Path.GetDirectoryName(Path.GetFullPath(solutionPath))!;
        var fileName = Path.GetFileName(solutionPath);

        var document = XDocument.Load(solutionPath, LoadOptions.None);

        var solutionElement = document.Root
            ?? throw new InvalidDataException($"'{fileName}' não possui elemento raiz.");

        var folders = new Dictionary<string, SolutionFolderEntry>(StringComparer.Ordinal);
        var projects = new List<ProjectEntry>();
        var limitations = new List<AcquisitionLimitation>();

        foreach (var folderElement in solutionElement.Elements("Folder"))
        {
            var rawName = (string?)folderElement.Attribute("Name");

            if (string.IsNullOrWhiteSpace(rawName))
            {
                limitations.Add(new AcquisitionLimitation
                {
                    Reason = "Pasta de solução sem atributo Name",
                    AffectedScope = "solution-folder",
                    Source = fileName
                });

                continue;
            }

            var logicalPath = PathNormalizer.Normalize(rawName);

            RegisterFolderChain(folders, logicalPath);

            foreach (var projectElement in folderElement.Elements("Project"))
            {
                var entry = ReadProject(projectElement, fileName, logicalPath, limitations);

                if (entry is not null)
                    projects.Add(entry);
            }
        }

        foreach (var projectElement in solutionElement.Elements("Project"))
        {
            var entry = ReadProject(projectElement, fileName, folderLogicalPath: null, limitations);

            if (entry is not null)
                projects.Add(entry);
        }

        return new SolutionFile
        {
            Name = Path.GetFileNameWithoutExtension(solutionPath),
            RootDirectory = root,
            FileName = fileName,
            Folders = folders.Values
                .OrderBy(f => f.LogicalPath, StringComparer.Ordinal)
                .ToArray(),
            Projects = projects
                .OrderBy(p => p.RelativePath, StringComparer.Ordinal)
                .ToArray(),
            Limitations = limitations
        };
    }

    /// <summary>"src/Core" cria também "src", para que a hierarquia seja completa.</summary>
    private static void RegisterFolderChain(
        Dictionary<string, SolutionFolderEntry> folders,
        string logicalPath)
    {
        var segments = logicalPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var accumulated = string.Empty;
        string? parent = null;

        foreach (var segment in segments)
        {
            accumulated = accumulated.Length == 0 ? segment : $"{accumulated}/{segment}";

            if (!folders.ContainsKey(accumulated))
            {
                folders[accumulated] = new SolutionFolderEntry
                {
                    LogicalPath = accumulated,
                    Name = segment,
                    ParentLogicalPath = parent
                };
            }

            parent = accumulated;
        }
    }

    private static ProjectEntry? ReadProject(
        XElement element,
        string source,
        string? folderLogicalPath,
        List<AcquisitionLimitation> limitations)
    {
        var rawPath = (string?)element.Attribute("Path");

        if (string.IsNullOrWhiteSpace(rawPath))
        {
            limitations.Add(new AcquisitionLimitation
            {
                Reason = "Projeto sem atributo Path",
                AffectedScope = "project",
                Source = source
            });

            return null;
        }

        var relativePath = PathNormalizer.Normalize(rawPath);

        return new ProjectEntry
        {
            Name = Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            FolderLogicalPath = folderLogicalPath
        };
    }
}
