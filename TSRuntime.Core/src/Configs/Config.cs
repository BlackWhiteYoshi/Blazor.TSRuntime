namespace TSRuntime.Core.Configs;

public sealed class Config {
    public readonly string DeclarationPath = @"../../../.typescript-declarations/";

    public readonly string FileOutputClass = "TSRuntime.cs";
    public readonly string FileOutputinterface = "ITSRuntime.cs";

    public readonly bool ModuleInvokeEnabled = true;
    public readonly bool ModuleTrySyncEnabled = true;
    public readonly bool ModuleAsyncEnabled = true;

    public readonly bool JSRuntimeInvokeEnabled = true;
    public readonly bool JSRuntimeTrySyncEnabled = true;
    public readonly bool JSRuntimeAsyncEnabled = true;


    public readonly FunctionNamePattern FunctionNamePattern = new("$function$_$module$_$action$");
    
    public string[] UsingStatements { get; } = new string[1] { "Microsoft.AspNetCore.Components" };

    public Dictionary<string, string> TypeMap { get; } = new() {
        ["number"] = "double",
        ["boolean"] = "bool",
        ["bigint"] = "long",
        ["HTMLObjectElement"] = "ElementReference"
    };
}
