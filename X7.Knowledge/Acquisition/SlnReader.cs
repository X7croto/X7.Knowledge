using System.Text.RegularExpressions;

namespace X7.Knowledge.Acquisition;

/// <summary>Leitor do formato .sln clássico.</summary>
internal static partial class SlnReader
{
    private const string SolutionFolderTypeGuid = "2150E333-8FDC-42A3-9474-1DEA7842C87C";

    [GeneratedRegex(
        """^Project\("\{(?<type>[^}]+)\}"\)\s*=\s*"(?<name>[^"]*)"\s*,\s*"(?<path>[^"]*)"\s*,\s*"\{(?<id>[^}]+)\}"\s*$""",
        RegexOptions.ExplicitCapture)]
    private static partial Regex ProjectLine();

    [GeneratedRegex(
        """^\s*\{(?<child>[^}]+)\}\s*=\s*\{(?<parent>[^}]+)\}\s*$""",
        RegexOptions.ExplicitCapture)]
    private static partial Regex NestingLine();

    private sealed record RawEntry(string Guid, string Name, string Path, bool IsFolder);

    public static SolutionFile Read(string solutionPath)
    {
        var root = Path.GetDirectoryName(Path.GetFullPath(solutionPath))!;
        var fileName = Path.GetFileName(solutionPath);

        var entries = new Dictionary<string, RawEntry>(StringComparer.OrdinalIgnoreCase);
        var nesting = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var limitations = new List<AcquisitionLimitation>();

        var inNestedSection = false;

        foreach (var raw in File.ReadLines(solutionPath))
        {
            var line = raw.Trim();

            if (line.StartsWith("GlobalSection(NestedProjects)", StringComparison.Ordinal))
            {
                inNestedSection = true;
                continue;
            }

            if (line.StartsWith("EndGlobalSection", StringComparison.Ordinal))
            {
                inNestedSection = false;
                continue;
            }

            if (inNestedSection)
            {
                var nestMatch = NestingLine().Match(line);

                if (nestMatch.Success)
                {
                    nesting[nestMatch.Groups["child"].Value] =
                        nestMatch.Groups["parent"].Value;
                }

                continue;
            }

            var match = ProjectLine().Match(line);

            if (!match.Success)
                continue;

            var isFolder = string.Equals(
                match.Groups["type"].Value,
                SolutionFolderTypeGuid,
                StringComparison.OrdinalIgnoreCase);

            entries[match.Groups["id"].Value] = new RawEntry(
                match.Groups["id"].Value,
                match.Groups["name"].Value,
                PathNormalizer.Normalize(match.Groups["path"].Value),
                isFolder);
        }

        var folders = new Dictionary<string, SolutionFolderEntry>(StringComparer.Ordinal);
        var logicalPathByGuid = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries.Values.Where(e => e.IsFolder))
        {
            var logicalPath = BuildLogicalPath(entry.Guid, entries, nesting);

            logicalPathByGuid[entry.Guid] = logicalPath;

            var parentGuid = nesting.GetValueOrDefault(entry.Guid);

            var parentPath = parentGuid is not null && entries.TryGetValue(parentGuid, out var p) && p.IsFolder
                ? BuildLogicalPath(parentGuid, entries, nesting)
                : null;

            folders[logicalPath] = new SolutionFolderEntry
            {
                LogicalPath = logicalPath,
                Name = entry.Name,
                ParentLogicalPath = parentPath
            };
        }

        var projects = new List<ProjectEntry>();

        foreach (var entry in entries.Values.Where(e => !e.IsFolder))
        {
            if (!entry.Path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                limitations.Add(new AcquisitionLimitation
                {
                    Reason = $"Projeto não-C# ignorado: '{entry.Path}'",
                    AffectedScope = "project",
                    Source = fileName
                });

                continue;
            }

            var parentGuid = nesting.GetValueOrDefault(entry.Guid);

            projects.Add(new ProjectEntry
            {
                Name = entry.Name,
                RelativePath = entry.Path,
                FolderLogicalPath = parentGuid is not null
                    ? logicalPathByGuid.GetValueOrDefault(parentGuid)
                    : null
            });
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

    private static string BuildLogicalPath(
        string guid,
        Dictionary<string, RawEntry> entries,
        Dictionary<string, string> nesting)
    {
        var segments = new List<string>();
        var current = guid;
        var guard = 0;

        while (entries.TryGetValue(current, out var entry) && entry.IsFolder)
        {
            segments.Insert(0, entry.Name);

            if (!nesting.TryGetValue(current, out var parent))
                break;

            current = parent;

            // Proteção contra aninhamento cíclico corrompido.
            if (++guard > 64)
                break;
        }

        return string.Join('/', segments);
    }
}
