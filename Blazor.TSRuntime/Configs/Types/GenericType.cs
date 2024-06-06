namespace TSRuntime.Configs;

public readonly record struct GenericType(string name) {
    public string Name { get; init; } = name;
    public string? Constraint { get; init; } = null;
}
