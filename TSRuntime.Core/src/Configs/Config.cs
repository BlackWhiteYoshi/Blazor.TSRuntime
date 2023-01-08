using System.Text;
using System.Text.Json.Nodes;
using TSRuntime.Core.Configs.NamePattern;

namespace TSRuntime.Core.Configs;

/// <summary>
/// The configurations for generating the ITSRuntime content.
/// </summary>
public sealed record class Config {
    /// <summary>
    /// <para>Folder where to locate the d.ts declaration files.</para>
    /// <para>Path relative to this file and no starting or ending slash.</para>
    /// </summary>
    public string DeclarationPath { get; init; } = DECLARATION_PATH;
    private const string DECLARATION_PATH = @".typescript-declarations";

    /// <summary>
    /// <para>File-path of TSRuntime.</para>
    /// <para>Path relative to this file and no starting slash.</para>
    /// <para>At the moment it is unused.</para>
    /// </summary>
    public string FileOutputClass { get; init; } = FILE_OUTPUT_CLASS;
    private const string FILE_OUTPUT_CLASS = "TSRuntime/TSRuntime.cs";
    /// <summary>
    /// <para>File-path of ITSRuntime.</para>
    /// <para>Path relative to this file and no starting slash.</para>
    /// <para>At the moment it is unused.</para>
    /// </summary>
    public string FileOutputinterface { get; init; } = FILE_OUTPUT_INTERFACE;
    private const string FILE_OUTPUT_INTERFACE = "TSRuntime/ITSRuntime.cs";

    /// <summary>
    /// Toggles whether sync invoke methods should be generated for modules.
    /// </summary>
    public bool ModuleInvokeEnabled { get; init; } = MODULE_INVOKE_ENABLED;
    private const bool MODULE_INVOKE_ENABLED = false;
    /// <summary>
    /// Toggles whether try-sync invoke methods should be generated for modules.
    /// </summary>
    public bool ModuleTrySyncEnabled { get; init; } = MODULE_TRYSYNC_ENABLED;
    private const bool MODULE_TRYSYNC_ENABLED = true;
    /// <summary>
    /// Toggles whether async invoke methods should be generated for modules.
    /// </summary>
    public bool ModuleAsyncEnabled { get; init; } = MODULE_ASYNC_ENABLED;
    private const bool MODULE_ASYNC_ENABLED = false;

    /// <summary>
    /// Toggles whether generic JSRuntime sync invoke method should be generated.
    /// </summary>
    public bool JSRuntimeInvokeEnabled { get; init; } = JSRUNTIME_INVOKE_ENABLED;
    private const bool JSRUNTIME_INVOKE_ENABLED = false;
    /// <summary>
    /// Toggles whether generic JSRuntime try-sync invoke method should be generated.
    /// </summary>
    public bool JSRuntimeTrySyncEnabled { get; init; } = JSRUNTIME_TRYSYNC_ENABLED;
    private const bool JSRUNTIME_TRYSYNC_ENABLED = false;
    /// <summary>
    /// Toggles whether generic JSRuntime async invoke method should be generated.
    /// </summary>
    public bool JSRuntimeAsyncEnabled { get; init; } = JSRUNTIME_ASYNC_ENABLED;
    private const bool JSRUNTIME_ASYNC_ENABLED = false;


    /// <summary>
    /// Naming of the generated methods that invoke module functions.
    /// </summary>
    public FunctionNamePattern FunctionNamePattern { get; init; } = new(FUNCTION_NAME_PATTERN, FUNCTION_TRANSFORM, MODULE_TRANSFORM, ACTION_TRANSFORM);
    private const string FUNCTION_NAME_PATTERN = "#function#";
    private const NameTransform FUNCTION_TRANSFORM = NameTransform.FirstUpperCase;
    private const NameTransform MODULE_TRANSFORM = NameTransform.None;
    private const NameTransform ACTION_TRANSFORM = NameTransform.None;

    /// <summary>
    /// Naming of the generated methods that pre loads a specific module.
    /// </summary>
    public PreLoadNamePattern PreLoadNamePattern { get; init; } = new(PRE_LOAD_NAME_PATTERN, PRE_LOAD_MODULE_TRANSFORM);
    private const string PRE_LOAD_NAME_PATTERN = "PreLoad#module#";
    private const NameTransform PRE_LOAD_MODULE_TRANSFORM = NameTransform.None;

    /// <summary>
    /// Naming of the generated method that pre loads all modules.
    /// </summary>
    public string PreLoadAllModulesName = PRE_LOAD_ALL_MODULES_NAME;
    private const string PRE_LOAD_ALL_MODULES_NAME = "AllModules";


    /// <summary>
    /// List of generated using statements at the top of ITSRuntime.
    /// </summary>
    public string[] UsingStatements { get; init; } = new string[1] { USING_STATEMENT };
    private const string USING_STATEMENT = "Microsoft.AspNetCore.Components";


    /// <summary>
    /// <para>Mapping of typescript-types (key) to C#-types (value).</para>
    /// <para>Not listed types are mapped unchanged (Identity function).</para>
    /// </summary>
    public Dictionary<string, string> TypeMap { get; init; } = TypeMapDefault;
    private static Dictionary<string, string> TypeMapDefault => new() {
        ["number"] = "double",
        ["boolean"] = "bool",
        ["bigint"] = "long",
        ["HTMLElement"] = "ElementReference"
    };


    #region json

    /// <summary>
    /// The name/path of the json-config file which is a representation of an instance of this class.
    /// </summary>
    public const string JSON_FILE_NAME = "tsconfig.tsruntime.json";

    /// <summary>
    /// Converts this instance as a json-file.
    /// </summary>
    public string ToJson() {
        string usingStatements = UsingStatements.Length switch {
            0 => string.Empty,
            1 => $""" "{UsingStatements[0]}" """,
            _ => $"""
                    
                        "{string.Join("""
                            ",
                                "
                            """, UsingStatements)}"
                      
                    """
        };

        string typeMap;
        if (TypeMap.Count == 0)
            typeMap = " ";
        else {
            StringBuilder typeMapBuilder = new(100);

            foreach(KeyValuePair<string, string> pair in TypeMap) {
                typeMapBuilder.Append("""

                        "
                    """);
                typeMapBuilder.Append(pair.Key);
                typeMapBuilder.Append("""
                    ": "
                    """);
                typeMapBuilder.Append(pair.Value);
                typeMapBuilder.Append("""
                    ",
                    """);
            }
            typeMapBuilder.Length--;
            typeMapBuilder.Append("""

              
            """);
            typeMap = typeMapBuilder.ToString();
        }

        return $$"""
            {
              "declaration path": "{{DeclarationPath}}",
              "file output": {
                "class": "{{FileOutputClass}}",
                "interface": "{{FileOutputinterface}}"
              },
              "module": {
                "invoke enabled": {{(ModuleInvokeEnabled ? "true" : "false")}},
                "trysync enabled": {{(ModuleTrySyncEnabled ? "true" : "false")}},
                "async enabled": {{(ModuleAsyncEnabled ? "true" : "false")}}
              },
              "js runtime": {
                "invoke enabled": {{(JSRuntimeInvokeEnabled ? "true" : "false")}},
                "trysync enabled": {{(JSRuntimeTrySyncEnabled ? "true" : "false")}},
                "async enabled": {{(JSRuntimeAsyncEnabled ? "true" : "false")}}
              },
              "function name pattern": {
                "pattern": "{{FunctionNamePattern.NamePattern}}",
                "function transform": "{{FunctionNamePattern.FunctionTransform}}",
                "module transform": "{{FunctionNamePattern.ModuleTransform}}",
                "action transform": "{{FunctionNamePattern.ActionTransform}}"
              },
              "preload name pattern": {
                "pattern": "{{PreLoadNamePattern.NamePattern}}",
                "module transform": "{{PreLoadNamePattern.ModuleTransform}}",
                "all modules name": "{{PRE_LOAD_ALL_MODULES_NAME}}"
              },
              "using statements": [{{usingStatements}}],
              "type map": {{{typeMap}}}
            }

            """;
    }

    /// <summary>
    /// Converts the given json-object as a <see cref="Config"/> instance.
    /// </summary>
    public static Config FromJson(string json) {
        JsonNode root = JsonNode.Parse(json) ?? throw new ArgumentException($"json is not in a valid format:\n{json}");

        string? declarationPath = (string?)root["declaration path"];
        if (declarationPath != null) {
            declarationPath = declarationPath.Replace('\\', '/');
            if (declarationPath[^1] == '/')
                declarationPath = declarationPath[..^1];
        }
        else
            declarationPath = DECLARATION_PATH;

        return new Config() {
            DeclarationPath = declarationPath,

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
            
            PreLoadNamePattern = new PreLoadNamePattern(
                (string?)root["preload name pattern"]?["pattern"] ?? PRE_LOAD_NAME_PATTERN,
                Enum.TryParse(((string?)root["preload name pattern"]?["module transform"])?.Replace(" ", ""), ignoreCase: true, out NameTransform preLoadModuleTransform) ? preLoadModuleTransform : PRE_LOAD_MODULE_TRANSFORM),

            PreLoadAllModulesName = (string?)root["preload name pattern"]?["all modules name"] ?? PRE_LOAD_ALL_MODULES_NAME,
            
            UsingStatements = root["using statements"]?.ToStringArray() ?? new string[1] { USING_STATEMENT },

            TypeMap = root["type map"]?.ToStringDictionary() ?? TypeMapDefault
        };
    }

    #endregion
}

file static class JsonNodeExtension {
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
