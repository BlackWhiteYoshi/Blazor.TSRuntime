namespace TSRuntime.Configs.NamePattern;

internal readonly record struct OutputBlock(Output Output, string Content) {
    public static implicit operator OutputBlock(Output output) => new(output, string.Empty);

    public static implicit operator OutputBlock(string content) => new(Output.String, content);
}
