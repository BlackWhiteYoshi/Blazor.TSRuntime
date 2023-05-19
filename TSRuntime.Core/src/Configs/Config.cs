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
    /// <para>Path relative to json-file and no starting or ending slash.</para>
    /// </summary>
    public DeclarationPath[] DeclarationPath { get; init; } = DeclarationPathDefault;
    private static readonly DeclarationPath[] DeclarationPathDefault = new DeclarationPath[1] { new(string.Empty) };

    /// <summary>
    /// <para>File-path of TSRuntime.</para>
    /// <para>Path relative to json-file and no starting slash.</para>
    /// <para>Not used in source generator.</para>
    /// </summary>
    public string FileOutputClass { get; init; } = FILE_OUTPUT_CLASS;
    private const string FILE_OUTPUT_CLASS = "TSRuntime/TSRuntime.cs";
    /// <summary>
    /// <para>File-path of ITSRuntime.</para>
    /// <para>Path relative to json-file and no starting slash.</para>
    /// <para>Not used in source generator.</para>
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
    /// <para>If true, whenever a module function returns a promise, the <see cref="ModuleInvokeEnabled" />, <see cref="ModuleTrySyncEnabled" /> and <see cref="ModuleAsyncEnabled" /> flags will be ignored<br />
    /// and instead only the async invoke method will be generated.</para>
    /// <para>This value should always be true. Set it only to false when you know what you are doing.</para>
    /// </summary>
    public bool PromiseFunctionOnlyAsync { get; init; } = PROMISE_FUNCTION_ONLY_ASYNC;
    private const bool PROMISE_FUNCTION_ONLY_ASYNC = true;
    /// <summary>
    /// <para>If true, whenever a module function returns a promise, the string "Async" is appended.</para>
    /// <para>If your pattern ends already with "Async", for example with the #action# variable, this will result in a double: "AsyncAsync"</para>
    /// </summary>
    public bool PromiseFunctionAppendAsync { get; init; } = PROMISE_FUNCTION_APPEND_ASYNC;
    private const bool PROMISE_FUNCTION_APPEND_ASYNC = false;

    /// <summary>
    /// Naming of the generated methods that invoke module functions.
    /// </summary>
    public FunctionNamePattern FunctionNamePattern { get; init; } = new(FUNCTION_NAME_PATTERN, MODULE_TRANSFORM, FUNCTION_TRANSFORM, ACTION_TRANSFORM);
    private const string FUNCTION_NAME_PATTERN = "#function#";
    private const NameTransform MODULE_TRANSFORM = NameTransform.FirstUpperCase;
    private const NameTransform FUNCTION_TRANSFORM = NameTransform.FirstUpperCase;
    private const NameTransform ACTION_TRANSFORM = NameTransform.None;

    /// <summary>
    /// Naming of the generated methods that preloads a specific module.
    /// </summary>
    public ModuleNamePattern PreloadNamePattern { get; init; } = new(PRELOAD_NAME_PATTERN, PRELOAD_MODULE_TRANSFORM);
    private const string PRELOAD_NAME_PATTERN = "Preload#module#";
    private const NameTransform PRELOAD_MODULE_TRANSFORM = NameTransform.FirstUpperCase;

    /// <summary>
    /// Naming of the method that preloads all modules.
    /// </summary>
    public string PreloadAllModulesName { get; init; } = PRELOAD_ALL_MODULES_NAME;
    private const string PRELOAD_ALL_MODULES_NAME = "PreloadAllModules";


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
        ["HTMLObjectElement"] = "ElementReference"
    };


    /// <summary>
    /// Compares all values when changed result in a change of the structureTree.
    /// </summary>
    /// <param name="other"></param>
    /// <remarks><see cref="DeclarationPath"/> is not included here, although it changes the structureTree, because it also changes parsing paths and therefore must be treated especially.</remarks>
    /// <returns>true, when all values are the same and thereby no change in the structureTree happened.</returns>
    public bool StructureTreeEquals(Config other) {
        if (ModuleInvokeEnabled != other.ModuleInvokeEnabled)
            return false;
        if (ModuleTrySyncEnabled != other.ModuleTrySyncEnabled)
            return false;
        if (ModuleAsyncEnabled != other.ModuleAsyncEnabled)
            return false;
        if (JSRuntimeInvokeEnabled != other.JSRuntimeInvokeEnabled)
            return false;
        if (JSRuntimeTrySyncEnabled != other.JSRuntimeTrySyncEnabled)
            return false;
        if (JSRuntimeAsyncEnabled != other.JSRuntimeAsyncEnabled)
            return false;
        if (PromiseFunctionOnlyAsync != other.PromiseFunctionOnlyAsync)
            return false;
        if (PromiseFunctionAppendAsync != other.PromiseFunctionAppendAsync)
            return false;
        if (FunctionNamePattern != other.FunctionNamePattern)
            return false;
        if (PreloadNamePattern != other.PreloadNamePattern)
            return false;
        if (PreloadAllModulesName != other.PreloadAllModulesName)
            return false;

        if (UsingStatements.Length != other.UsingStatements.Length)
            return false;
        for (int i = 0; i < UsingStatements.Length; i++)
            if (UsingStatements[i] != other.UsingStatements[i])
                return false;

        if (TypeMap.Count != other.TypeMap.Count)
            return false;        
        foreach (KeyValuePair<string, string> pair in TypeMap) {
            if (!other.TypeMap.TryGetValue(pair.Key, out string value))
                return false;

            if (pair.Value != value)
                return false;
        }

        return true;
    }


    #region json

    /// <summary>
    /// The name/path of the json-config file which is a representation of an instance of this class.
    /// </summary>
    public const string JSON_FILE_NAME = "tsconfig.tsruntime.json";

    /// <summary>
    /// Converts this instance as a json-file.
    /// </summary>
    public string ToJson() {
        StringBuilder builder = new(100);


        string declarationPath;
        if (DeclarationPath.Length == 0)
            declarationPath = string.Empty;
        else {
            builder.Clear();

            foreach ((string include, string[] excludes, string? fileModulePath) in DeclarationPath) {
                builder.Append($$"""
                    
                        {
                          "include": "{{include}}",
                          "excludes": {{excludes.Length switch {
                    0 => "[]",
                    1 => $"""[ "{excludes[0]}" ]""",
                    _ => $"""
                        [
                                "{string.Join("\",\n        \"", excludes)}"
                              ]
                        """
                }}}
                    """);
                if (fileModulePath != null)
                    builder.Append($"""
                        ,
                              "file module path": "{fileModulePath}"
                        """);
                builder.Append("\n    },");
            }

            builder.Length--;
            builder.Append("\n  ");
            declarationPath = builder.ToString();
        }

        string usingStatements = UsingStatements.Length switch {
            0 => string.Empty,
            1 => $""" "{UsingStatements[0]}" """,
            _ => $"""
                    
                        "{string.Join("\",\n    \"", UsingStatements)}"
                      
                    """
        };

        string typeMap;
        if (TypeMap.Count == 0)
            typeMap = " ";
        else {
            builder.Clear();

            foreach (KeyValuePair<string, string> pair in TypeMap) {
                builder.Append("""

                        "
                    """);
                builder.Append(pair.Key);
                builder.Append("""
                    ": "
                    """);
                builder.Append(pair.Value);
                builder.Append("""
                    ",
                    """);
            }
            builder.Length--;
            builder.Append("""

              
            """);
            typeMap = builder.ToString();
        }

        return $$"""
            {
              "declaration path": [{{declarationPath}}],
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
              "promise function": {
                "only async enabled": {{(PromiseFunctionOnlyAsync ? "true" : "false")}},
                "append Async": {{(PromiseFunctionAppendAsync ? "true" : "false")}}
              },
              "function name pattern": {
                "pattern": "{{FunctionNamePattern.NamePattern}}",
                "module transform": "{{FunctionNamePattern.ModuleTransform}}",
                "function transform": "{{FunctionNamePattern.FunctionTransform}}",
                "action transform": "{{FunctionNamePattern.ActionTransform}}"
              },
              "preload name pattern": {
                "pattern": "{{PreloadNamePattern.NamePattern}}",
                "module transform": "{{PreloadNamePattern.ModuleTransform}}"
              },
              "preload all modules name": "{{PRELOAD_ALL_MODULES_NAME}}",
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

        return new Config() {
            DeclarationPath = root["declaration path"]?.ParseDeclarationPath() ?? DeclarationPathDefault,

            FileOutputClass = (string?)root["file output"]?["class"] ?? FILE_OUTPUT_CLASS,
            FileOutputinterface = (string?)root["file output"]?["interface"] ?? FILE_OUTPUT_INTERFACE,

            ModuleInvokeEnabled = (bool?)root["module"]?["invoke enabled"] ?? MODULE_INVOKE_ENABLED,
            ModuleTrySyncEnabled = (bool?)root["module"]?["trysync enabled"] ?? MODULE_TRYSYNC_ENABLED,
            ModuleAsyncEnabled = (bool?)root["module"]?["async enabled"] ?? MODULE_ASYNC_ENABLED,

            JSRuntimeInvokeEnabled = (bool?)root["js runtime"]?["invoke enabled"] ?? JSRUNTIME_INVOKE_ENABLED,
            JSRuntimeTrySyncEnabled = (bool?)root["js runtime"]?["trysync enabled"] ?? JSRUNTIME_TRYSYNC_ENABLED,
            JSRuntimeAsyncEnabled = (bool?)root["js runtime"]?["async enabled"] ?? JSRUNTIME_ASYNC_ENABLED,

            PromiseFunctionOnlyAsync = (bool?)root["promise function"]?["only async enabled"] ?? PROMISE_FUNCTION_ONLY_ASYNC,
            PromiseFunctionAppendAsync = (bool?)root["promise function"]?["append Async"] ?? PROMISE_FUNCTION_APPEND_ASYNC,

            FunctionNamePattern = new FunctionNamePattern(
                (string?)root["function name pattern"]?["pattern"] ?? FUNCTION_NAME_PATTERN,
                Enum.TryParse(((string?)root["function name pattern"]?["module transform"])?.Replace(" ", ""), ignoreCase: true, out NameTransform moduleTransform) ? moduleTransform : MODULE_TRANSFORM,
                Enum.TryParse(((string?)root["function name pattern"]?["function transform"])?.Replace(" ", ""), ignoreCase: true, out NameTransform functionTransform) ? functionTransform : FUNCTION_TRANSFORM,
                Enum.TryParse(((string?)root["function name pattern"]?["action transform"])?.Replace(" ", ""), ignoreCase: true, out NameTransform actionTransform) ? actionTransform : ACTION_TRANSFORM),
            
            PreloadNamePattern = new ModuleNamePattern(
                (string?)root["preload name pattern"]?["pattern"] ?? PRELOAD_NAME_PATTERN,
                Enum.TryParse(((string?)root["preload name pattern"]?["module transform"])?.Replace(" ", ""), ignoreCase: true, out NameTransform preLoadModuleTransform) ? preLoadModuleTransform : PRELOAD_MODULE_TRANSFORM),
            PreloadAllModulesName = (string?)root["preload all modules name"] ?? PRELOAD_ALL_MODULES_NAME,
            
            UsingStatements = root["using statements"]?.ToStringArray() ?? new string[1] { USING_STATEMENT },

            TypeMap = root["type map"]?.ToStringDictionary() ?? TypeMapDefault
        };
    }

    #endregion
}

file static class JsonNodeExtension {
    internal static string[] ToStringArray(this JsonNode node) {
        switch (node) {
            case JsonArray array:
                string[] result = new string[array.Count];

                for (int i = 0; i < array.Count; i++)
                    result[i] = (string?)array[i] ?? throw NullNotAllowed;

                return result;

            case JsonNode valueNode:
                return new string[1] { (string?)valueNode ?? throw NullNotAllowed };
            
            default:
                throw NullNotAllowed;
        };
    }

    internal static Dictionary<string, string> ToStringDictionary(this JsonNode node) {
        JsonObject jsonObject = node.AsObject();
        Dictionary<string, string> result = new(jsonObject.Count);

        foreach (KeyValuePair<string, JsonNode?> item in jsonObject)
            result.Add(item.Key, (string?)item.Value ?? throw NullNotAllowed);

        return result;
    }

    internal static DeclarationPath[] ParseDeclarationPath(this JsonNode node) {
        switch (node) {
            case JsonArray array:
                DeclarationPath[] result = new DeclarationPath[array.Count];

                for (int i = 0; i < array.Count; i++)
                    if (array[i] is JsonObject jsonObject)
                        result[i] = ParseJsonObject(jsonObject);
                    else
                        result[i] = new DeclarationPath(GetPath(array[i]));

                return result;

            case JsonObject jsonObject:
                return new DeclarationPath[1] { ParseJsonObject(jsonObject) };

            default:
                return new DeclarationPath[1] { new(GetPath(node)) };
        }


        static DeclarationPath ParseJsonObject(JsonObject jsonObject) {
            string? fileModulePath = (string?)jsonObject["file module path"];
            if (fileModulePath != null)
                fileModulePath = Normalize(fileModulePath);


            if (!jsonObject.TryGetPropertyValue("include", out JsonNode? includeNode))
                throw new ArgumentException("Key \"include\" not found in a DeclarationPath object.");

            string include = GetPath(includeNode);


            if (!jsonObject.TryGetPropertyValue("excludes", out JsonNode? excludeNode))
                return new DeclarationPath(include, fileModulePath);

            string[] excludes = excludeNode!.ToStringArray();
            for (int i = 0; i < excludes.Length; i++)
                excludes[i] = Normalize(excludes[i]);


            return new DeclarationPath(include, excludes, fileModulePath); 
        }

        static string GetPath(JsonNode? node) {
            string path = (string?)node ?? throw NullNotAllowed;
            return Normalize(path);
        }

        // replaces '\' with '/' and removes trailing slash
        static string Normalize(string path) {
            path = path.Replace('\\', '/');

            if (path is [.., '/'])
                path = path[..^1];

            return path;
        }
    }


    private static ArgumentException NullNotAllowed => new("non-string literals and null is not allowed - use string literal \"null\" instead");
}
