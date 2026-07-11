namespace X7.Knowledge;

public sealed class Identity : IEquatable<Identity>
{
    private Identity(string id)
    {
        Id = id;
    }

    public string Id { get; }

    public static Identity Create(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return new Identity(id);
    }

    public bool Equals(Identity? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return StringComparer.Ordinal.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
        => Equals(obj as Identity);

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Id);

    public override string ToString()
        => Id;
}