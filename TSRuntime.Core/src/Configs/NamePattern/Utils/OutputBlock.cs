namespace TSRuntime.Core.Configs.NamePattern;

internal struct OutputBlock {
    public Output output;
    public string content;

    public static implicit operator OutputBlock(Output output) => new() {
        output = output,
        content = string.Empty
    };

    public static implicit operator OutputBlock(string content) => new() {
        output = Output.String,
        content = content
    };
}
