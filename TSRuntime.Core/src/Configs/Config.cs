namespace TSRuntime.Core.Configs;

public sealed class Config {
    public const string DECLARATION_PATH = @"../../../.typescript-declarations/";

    public readonly string FileOutputClass = "TSRuntime.cs";
    public readonly string FileOutputinterface = "ITSRuntime.cs";

    public Dictionary<string, string> TypeMap { get; } = new() {
        ["number"] = "double",
        ["boolean"] = "bool",
        ["bigint"] = "long",
        ["HTMLObjectElement"] = "ElementReference"
    };

    public string[] UsingStatements { get; } = new string[1] { "Microsoft.AspNetCore.Components" };
}
