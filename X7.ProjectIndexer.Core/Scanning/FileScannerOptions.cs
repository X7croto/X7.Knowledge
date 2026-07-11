namespace X7.ProjectIndexer.Core.Scanning;

public sealed class FileScannerOptions
{
    public HashSet<string> IgnoredDirectories { get; } =
    [
        ".git",
        ".vs",
        "bin",
        "obj",
        "packages",
        "node_modules"
    ];

    public HashSet<string> Extensions { get; } =
    [
        ".cs",
        ".xaml",
        ".axaml",
        ".csproj",
        ".sln",
        ".json",
        ".xml",
        ".md"
    ];
}