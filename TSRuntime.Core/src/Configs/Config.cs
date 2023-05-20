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
    public FunctionNamePattern FunctionNamePattern { get; init; } = new(FUNCTION_NAME_PATTERN, FUNCTION_MODULE_TRANSFORM, FUNCTION_FUNCTION_TRANSFORM, FUNCTION_ACTION_TRANSFORM);
    private const string FUNCTION_NAME_PATTERN = "#function#";
    private const NameTransform FUNCTION_MODULE_TRANSFORM = NameTransform.FirstUpperCase;
    private const NameTransform FUNCTION_FUNCTION_TRANSFORM = NameTransform.FirstUpperCase;
    private const NameTransform FUNCTION_ACTION_TRANSFORM = NameTransform.None;

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


        DeclarationPath[] declarationPath;
        {
            try {
                declarationPath = root["declaration path"] switch {
                    JsonArray array => ParseJsonArray(array),
                    JsonObject jsonObject => new DeclarationPath[1] { ParseJsonObject(jsonObject) },
                    JsonValue value => new DeclarationPath[1] { new(Normalize(value.ParseAsString("declaration path"))) },
                    null => DeclarationPathDefault,
                    _ => throw JsonException.UnexpectedType("declaration path")
                };
            }
            catch (ArgumentException exception) { throw new ArgumentException($"invalid declaration path: {exception.Message}", exception); }


            static DeclarationPath[] ParseJsonArray(JsonArray array) {
                DeclarationPath[] result = new DeclarationPath[array.Count];

                for (int i = 0; i < array.Count; i++)
                    try {
                        result[i] = array[i] switch {
                            JsonObject jsonObject => ParseJsonObject(jsonObject),
                            JsonValue value => new DeclarationPath(Normalize(value.ParseAsString("declaration path"))),
                            _ => throw JsonException.UnexpectedType("declaration path")
                        };
                    }
                    catch (ArgumentException exception) { throw new ArgumentException($"error at array index {i}: {exception.Message}", exception); }

                return result;
            }

            static DeclarationPath ParseJsonObject(JsonObject jsonObject) {
                string include = jsonObject["include"] switch {
                    JsonValue value => Normalize(value.ParseAsString("include")),
                    null => throw JsonException.KeyNotFound("include"),
                    _ => throw JsonException.UnexpectedType("include")
                };

                string[] excludes = jsonObject.ParseAsStringArray("excludes") ?? Array.Empty<string>();
                for (int i = 0; i < excludes.Length; i++)
                    excludes[i] = Normalize(excludes[i]);

                string? fileModulePath = jsonObject["file module path"] switch {
                    JsonValue value => Normalize(value.ParseAsString("file module path")),
                    null => null,
                    _ => throw JsonException.UnexpectedType("file module path")
                };

                return new DeclarationPath(include, excludes, fileModulePath);
            }

            // replaces '\' with '/' and removes trailing slash
            static string Normalize(string path) {
                path = path.Replace('\\', '/');

                if (path is [.., '/'])
                    path = path[..^1];

                return path;
            }
        }

        string fileOutputClass;
        string fileOutputinterface;
        {
            if (root.AsJsonObjectOrNull("file output") is JsonObject jsonObject) {
                fileOutputClass = jsonObject["class"].ParseAsString("[file output].class");
                fileOutputinterface = jsonObject["interface"].ParseAsString("[file output].interface");
            }
            else {
                fileOutputClass = FILE_OUTPUT_CLASS;
                fileOutputinterface = FILE_OUTPUT_INTERFACE;
            }
        }

        bool moduleInvokeEnabled;
        bool moduleTrySyncEnabled;
        bool moduleAsyncEnabled;
        {
            if (root.AsJsonObjectOrNull("module") is JsonObject jsonObject) {
                moduleInvokeEnabled = jsonObject["invoke enabled"].ParseAsBool("module.[invoke enabled]");
                moduleTrySyncEnabled = jsonObject["trysync enabled"].ParseAsBool("module.[trysync enabled]");
                moduleAsyncEnabled = jsonObject["async enabled"].ParseAsBool("module.[async enabled]");
            }
            else {
                moduleInvokeEnabled = MODULE_INVOKE_ENABLED;
                moduleTrySyncEnabled = MODULE_TRYSYNC_ENABLED;
                moduleAsyncEnabled = MODULE_ASYNC_ENABLED;
            }
        }

        bool jsRuntimeInvokeEnabled;
        bool jsRuntimeTrySyncEnabled;
        bool jsRuntimeAsyncEnabled;
        {
            if (root.AsJsonObjectOrNull("js runtime") is JsonObject jsonObject) {
                jsRuntimeInvokeEnabled = jsonObject["invoke enabled"].ParseAsBool("[js runtime].[invoke enabled]");
                jsRuntimeTrySyncEnabled = jsonObject["trysync enabled"].ParseAsBool("[js runtime].[trysync enabled]");
                jsRuntimeAsyncEnabled = jsonObject["async enabled"].ParseAsBool("[js runtime].[async enabled]");
            }
            else {
                jsRuntimeInvokeEnabled = JSRUNTIME_INVOKE_ENABLED;
                jsRuntimeTrySyncEnabled = JSRUNTIME_TRYSYNC_ENABLED;
                jsRuntimeAsyncEnabled = JSRUNTIME_ASYNC_ENABLED;
            }
        }

        bool promiseFunctionOnlyAsync;
        bool promiseFunctionAppendAsync;
        {
            if (root.AsJsonObjectOrNull("promise function") is JsonObject jsonObject) {
                promiseFunctionOnlyAsync = jsonObject["only async enabled"].ParseAsBool("[promise function].[only async enabled]");
                promiseFunctionAppendAsync = jsonObject["append Async"].ParseAsBool("[promise function].[append Async]");
            }
            else {
                promiseFunctionOnlyAsync = PROMISE_FUNCTION_ONLY_ASYNC;
                promiseFunctionAppendAsync = PROMISE_FUNCTION_APPEND_ASYNC;
            }
        }

        string functionNamePattern;
        NameTransform functionModuleTransform;
        NameTransform functionFunctionTransform;
        NameTransform functionActionTransform;
        {
            if (root.AsJsonObjectOrNull("function name pattern") is JsonObject jsonObject) {
                functionNamePattern = jsonObject["pattern"].ParseAsString("[function name pattern].pattern");
                functionModuleTransform = jsonObject["module transform"].ParseAsNameTransform("[function name pattern].[module transform]");
                functionFunctionTransform = jsonObject["function transform"].ParseAsNameTransform("[function name pattern].[function transform]");
                functionActionTransform = jsonObject["action transform"].ParseAsNameTransform("[function name pattern].[action transform]");
            }
            else {
                functionNamePattern = FUNCTION_NAME_PATTERN;
                functionModuleTransform = FUNCTION_MODULE_TRANSFORM;
                functionFunctionTransform = FUNCTION_FUNCTION_TRANSFORM;
                functionActionTransform = FUNCTION_ACTION_TRANSFORM;
            }
        }

        string preloadNamePattern;
        NameTransform preloadModuleTransform;
        {
            if (root.AsJsonObjectOrNull("preload name pattern") is JsonObject jsonObject) {
                preloadNamePattern = jsonObject["pattern"].ParseAsString("[preload name pattern].pattern");
                preloadModuleTransform = jsonObject["module transform"].ParseAsNameTransform("[preload name pattern].[module transform]");
            }
            else {
                preloadNamePattern = PRELOAD_NAME_PATTERN;
                preloadModuleTransform = PRELOAD_MODULE_TRANSFORM;
            }
        }

        string preloadAllModulesName = root["preload all modules name"] switch {
            JsonValue jsonValue => jsonValue.ParseAsString("preload all modules name"),
            null => PRELOAD_ALL_MODULES_NAME,
            _ => throw JsonException.UnexpectedType("preload all modules name")
        };

        string[] usingStatements = root.ParseAsStringArray("using statements") ?? new string[1] { USING_STATEMENT };

        Dictionary<string, string> typeMap = root.ParseAsStringDictionary("type map") ?? TypeMapDefault;


        return new Config() {
            DeclarationPath = declarationPath,

            FileOutputClass = fileOutputClass,
            FileOutputinterface = fileOutputinterface,

            ModuleInvokeEnabled = moduleInvokeEnabled,
            ModuleTrySyncEnabled = moduleTrySyncEnabled,
            ModuleAsyncEnabled = moduleAsyncEnabled,

            JSRuntimeInvokeEnabled = jsRuntimeInvokeEnabled,
            JSRuntimeTrySyncEnabled = jsRuntimeTrySyncEnabled,
            JSRuntimeAsyncEnabled = jsRuntimeAsyncEnabled,

            PromiseFunctionOnlyAsync = promiseFunctionOnlyAsync,
            PromiseFunctionAppendAsync = promiseFunctionAppendAsync,

            FunctionNamePattern = new FunctionNamePattern(functionNamePattern, functionModuleTransform, functionFunctionTransform, functionActionTransform),

            PreloadNamePattern = new ModuleNamePattern(preloadNamePattern, preloadModuleTransform),
            PreloadAllModulesName = preloadAllModulesName,

            UsingStatements = usingStatements,

            TypeMap = typeMap
        };
    }

    #endregion
}

file static class JsonNodeExtension {
    internal static JsonObject? AsJsonObjectOrNull(this JsonNode parentNode, string key, string? parentkey = null)
        => parentNode[key] switch {
            JsonObject jsonObject => jsonObject,
            null => null,
            _ => throw JsonException.UnexpectedType(parentkey ?? key)
        };
    
    internal static string[]? ParseAsStringArray(this JsonNode parentNode, string parentKey) {
        return parentNode[parentKey] switch {
            JsonArray array => array.ParseAsStringArray(parentKey),
            JsonValue valueNode => new string[1] { valueNode.ParseAsString(parentKey) },
            null => null,
            _ => throw JsonException.UnexpectedType(parentKey)
        };
    }
    internal static string[] ParseAsStringArray(this JsonArray array, string parentKey) {
        string[] result = new string[array.Count];

        for (int i = 0; i < array.Count; i++)
            try {
                result[i] = array[i].ParseAsString(parentKey);
            }
            catch (ArgumentException exception) { throw new ArgumentException($"{exception.Message}, at array index {i}", exception); }

        return result;
    }

    internal static Dictionary<string, string>? ParseAsStringDictionary(this JsonNode parentNode, string parentKey) {
        return parentNode[parentKey] switch {
            JsonObject jsonObject => jsonObject.ParseAsStringDictionary(parentKey),
            null => null,
            _ => throw JsonException.UnexpectedType(parentKey)
        };
    }
    internal static Dictionary<string, string> ParseAsStringDictionary(this JsonObject jsonObject, string parentKey) {
        Dictionary<string, string> result = new(jsonObject.Count);

        foreach (KeyValuePair<string, JsonNode?> item in jsonObject)
            try {
                result.Add(item.Key, item.Value.ParseAsString(parentKey));
            }
            catch (ArgumentException exception) { throw new ArgumentException($"error at key element {item.Key}: {exception.Message}", exception); }

        return result;
    }

    internal static string ParseAsString(this JsonNode? node, string key) => ParseAsString(node as JsonValue ?? throw JsonException.UnexpectedType(key), key);
    internal static string ParseAsString(this JsonValue value, string key) => (string?)value ?? throw new ArgumentException($"""'{key}': must be a string. If you want to have null, use string literal "null" instead""");

    internal static bool ParseAsBool(this JsonNode? node, string key) => ParseAsBool(node as JsonValue ?? throw JsonException.UnexpectedType(key), key);
    internal static bool ParseAsBool(this JsonValue value, string key) => (bool?)value ?? throw new ArgumentException($@"'{key}': must be either ""true"" or ""false""");

    internal static NameTransform ParseAsNameTransform(this JsonNode? node, string key) => ParseAsNameTransform(node as JsonValue ?? throw JsonException.UnexpectedType(key), key);
    internal static NameTransform ParseAsNameTransform(this JsonValue value, string key) {
        const string errorMessage = @"must be either ""none"", ""first upper case"", ""first lower case"", ""upper case"" or ""lower case""";

        string str = (string?)value ?? throw new ArgumentException($"'{key}': {errorMessage}");
        string normaliezedStr = str.Replace(" ", "");

        bool success = Enum.TryParse(normaliezedStr, ignoreCase: true, out NameTransform nameTransform);
        if (!success)
            throw new ArgumentException($"'{key}': {errorMessage}");

        return nameTransform;
    }
}

file static class JsonException {
    internal static ArgumentException UnexpectedType(string key) => new($"'{key}': unexpected type");

    internal static ArgumentException KeyNotFound(string key) => new($"'{key}': not found");
}
