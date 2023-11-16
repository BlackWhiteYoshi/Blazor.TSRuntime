namespace TSRuntime.Core.Configs;

public readonly record struct GenericType(string name) {
    public readonly string Name { get; init; } = name;
    public readonly string? Constraint { get; init; } = null;
}
