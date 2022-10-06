using System.Text.Json.Nodes;

namespace TSRuntime.Core.Configs;

internal static class JsonNodeExtension {
    internal static string[] ToStringArray(this JsonNode node) => node.AsArray().Select((JsonNode? node) => (string?)node ?? throw NullNotAllowed).ToArray();

    internal static Dictionary<string, string> ToStringDictionary(this JsonNode node) {
        JsonObject jsonObject = node.AsObject();
        Dictionary<string, string> result = new(jsonObject.Count);

        foreach (KeyValuePair<string, JsonNode?> item in jsonObject)
            result.Add(item.Key, (string?)item.Value ?? throw NullNotAllowed);

        return result;
    }

    private static ArgumentException NullNotAllowed => new("null is not allowed - use string literal \"null\" instead");
}

public sealed class Config {
    public string DeclarationPath { get; init; } = DECLARATION_PATH;
    private const string DECLARATION_PATH = @"../../../.typescript-declarations/";

    public string FileOutputClass { get; init; } = FILE_OUTPUT_CLASS;
    private const string FILE_OUTPUT_CLASS = "TSRuntime.cs";
    public string FileOutputinterface { get; init; } = FILE_OUTPUT_INTERFACE;
    private const string FILE_OUTPUT_INTERFACE = "ITSRuntime.cs";

    public bool ModuleInvokeEnabled { get; init; } = MODULE_INVOKE_ENABLED;
    private const bool MODULE_INVOKE_ENABLED = true;
    public bool ModuleTrySyncEnabled { get; init; } = MODULE_TRYSYNC_ENABLED;
    private const bool MODULE_TRYSYNC_ENABLED = true;
    public bool ModuleAsyncEnabled { get; init; } = MODULE_ASYNC_ENABLED;
    private const bool MODULE_ASYNC_ENABLED = true;

    public bool JSRuntimeInvokeEnabled { get; init; } = JSRUNTIME_INVOKE_ENABLED;
    private const bool JSRUNTIME_INVOKE_ENABLED = true;
    public bool JSRuntimeTrySyncEnabled { get; init; } = JSRUNTIME_TRYSYNC_ENABLED;
    private const bool JSRUNTIME_TRYSYNC_ENABLED = true;
    public bool JSRuntimeAsyncEnabled { get; init; } = JSRUNTIME_ASYNC_ENABLED;
    private const bool JSRUNTIME_ASYNC_ENABLED = true;


    public FunctionNamePattern FunctionNamePattern { get; init; } = new(FUNCTION_NAME_PATTERN, FUNCTION_TRANSFORM, MODULE_TRANSFORM, ACTION_TRANSFORM);
    private const string FUNCTION_NAME_PATTERN = "$function$_$module$_$action$";
    private const NameTransform FUNCTION_TRANSFORM = NameTransform.FirstUpperCase;
    private const NameTransform MODULE_TRANSFORM = NameTransform.None;
    private const NameTransform ACTION_TRANSFORM = NameTransform.None;

    public string[] UsingStatements { get; init; } = new string[1] { USING_STATEMENT };
    private const string USING_STATEMENT = "Microsoft.AspNetCore.Components";

    public Dictionary<string, string> TypeMap { get; init; } = TypeMapDefault;
    private static Dictionary<string, string> TypeMapDefault => new() {
        ["number"] = "double",
        ["boolean"] = "bool",
        ["bigint"] = "long",
        ["HTMLObjectElement"] = "ElementReference"
    };


    public static Config FromJson(string json) {
        JsonNode root = JsonNode.Parse(json) ?? throw new ArgumentException($"json is not in a valid format:\n{json}");

        return new Config() {
            DeclarationPath = (string?)root["declaration path"] ?? DECLARATION_PATH,

            FileOutputClass = (string?)root["file output"]?["class"] ?? FILE_OUTPUT_CLASS,
            FileOutputinterface = (string?)root["file output"]?["interface"] ?? FILE_OUTPUT_INTERFACE,

            ModuleInvokeEnabled = (bool?)root["module"]?["invoke enabled"] ?? MODULE_INVOKE_ENABLED,
            ModuleTrySyncEnabled = (bool?)root["module"]?["trysync enabled"] ?? MODULE_TRYSYNC_ENABLED,
            ModuleAsyncEnabled = (bool?)root["module"]?["async enabled"] ?? MODULE_ASYNC_ENABLED,

            JSRuntimeInvokeEnabled = (bool?)root["js runtime"]?["invoke enabled"] ?? JSRUNTIME_INVOKE_ENABLED,
            JSRuntimeTrySyncEnabled = (bool?)root["js runtime"]?["trysync enabled"] ?? JSRUNTIME_TRYSYNC_ENABLED,
            JSRuntimeAsyncEnabled = (bool?)root["js runtime"]?["async enabled"] ?? JSRUNTIME_ASYNC_ENABLED,

            FunctionNamePattern = new FunctionNamePattern(
                (string?)root["function name pattern"]?["pattern"] ?? FUNCTION_NAME_PATTERN,
                Enum.TryParse(((string?)root["function name pattern"]?["function transform"])?.Replace(" ", ""), ignoreCase: true, out NameTransform functionTransform) ? functionTransform : FUNCTION_TRANSFORM,
                Enum.TryParse(((string?)root["function name pattern"]?["module transform"])?.Replace(" ", ""), ignoreCase: true, out NameTransform moduleTransform) ? moduleTransform : MODULE_TRANSFORM,
                Enum.TryParse(((string?)root["function name pattern"]?["action transform"])?.Replace(" ", ""), ignoreCase: true, out NameTransform actionTransform) ? actionTransform : ACTION_TRANSFORM),

            UsingStatements = root["using statements"]?.ToStringArray() ?? new string[1] { USING_STATEMENT },

            TypeMap = root["type map"]?.ToStringDictionary() ?? TypeMapDefault
        };
    }
}
