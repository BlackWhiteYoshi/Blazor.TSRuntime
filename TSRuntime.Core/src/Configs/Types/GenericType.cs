namespace TSRuntime.Core.Configs;

public readonly record struct GenericType
{
    public readonly string Name { get; init; }
    public readonly string? Constraint { get; init; }

    public GenericType(string name)
    {
        Name = name;
        Constraint = null;
    }
}
