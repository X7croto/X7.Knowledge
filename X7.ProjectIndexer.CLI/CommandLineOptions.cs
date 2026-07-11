namespace X7.ProjectIndexer.CLI;

public sealed class CommandLineOptions
{
    public required string Input { get; init; }

    public string Output { get; init; } = ".x7index";

    public bool Verbose { get; init; }

    public bool Force { get; init; }
}