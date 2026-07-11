public sealed class DependencyInjectionMap
{
    public List<DIRegistration> Registrations { get; } = [];
}

public sealed class DIRegistration
{
    public string Service { get; init; } = "";

    public string Implementation { get; init; } = "";

    public string Lifetime { get; init; } = "";
}