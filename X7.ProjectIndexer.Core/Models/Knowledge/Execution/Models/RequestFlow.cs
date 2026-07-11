public sealed class RequestFlow
{
    public string Route { get; init; } = "";

    public string HttpMethod { get; init; } = "";

    public string Controller { get; init; } = "";

    public string Action { get; init; } = "";

    public List<string> Calls { get; } = [];

    public List<string> Repositories { get; } = [];

    public List<string> Entities { get; } = [];

    public List<string> Transactions { get; } = [];

    public List<string> Events { get; } = [];
}