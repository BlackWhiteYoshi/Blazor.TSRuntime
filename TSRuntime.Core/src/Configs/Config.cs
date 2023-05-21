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
    /// <para>If true, every time a .d.ts-file is changed, ITSRuntime is generated.</para>
    /// <para>Not used in source generator.</para>
    /// </summary>
    public bool GenerateOnSave { get; init; } = GENERATE_ON_SAVE;
    private const bool GENERATE_ON_SAVE = true;


    /// <summary>
    /// List of generated using statements at the top of ITSRuntime.
    /// </summary>
    public string[] UsingStatements { get; init; } = new string[1] { USING_STATEMENT };
    private const string USING_STATEMENT = "Microsoft.AspNetCore.Components";


    #region invoke function

    /// <summary>
    /// Toggles whether sync invoke methods should be generated for modules.
    /// </summary>
    public bool InvokeFunctionSyncEnabled { get; init; } = INVOKE_FUNCTION_SYNC_ENABLED;
    private const bool INVOKE_FUNCTION_SYNC_ENABLED = false;
    
    /// <summary>
    /// Toggles whether try-sync invoke methods should be generated for modules.
    /// </summary>
    public bool InvokeFunctionTrySyncEnabled { get; init; } = INVOKE_FUNCTION_TRYSYNC_ENABLED;
    private const bool INVOKE_FUNCTION_TRYSYNC_ENABLED = true;
    
    /// <summary>
    /// Toggles whether async invoke methods should be generated for modules.
    /// </summary>
    public bool InvokeFunctionAsyncEnabled { get; init; } = INVOKE_FUNCTION_ASYNC_ENABLED;
    private const bool INVOKE_FUNCTION_ASYNC_ENABLED = false;


    /// <summary>
    /// Naming of the generated methods that invoke module functions.
    /// </summary>
    public FunctionNamePattern InvokeFunctionNamePattern { get; init; } = new(INVOKE_FUNCTION_NAME_PATTERN, INVOKE_FUNCTION_MODULE_TRANSFORM, INVOKE_FUNCTION_FUNCTION_TRANSFORM, INVOKE_FUNCTION_ACTION_TRANSFORM);
    private const string INVOKE_FUNCTION_NAME_PATTERN = "#function#";
    private const NameTransform INVOKE_FUNCTION_MODULE_TRANSFORM = NameTransform.FirstUpperCase;
    private const NameTransform INVOKE_FUNCTION_FUNCTION_TRANSFORM = NameTransform.FirstUpperCase;
    private const NameTransform INVOKE_FUNCTION_ACTION_TRANSFORM = NameTransform.None;

    /// <summary>
    /// Naming of the #action# variable for the invoke module functions name pattern when the action is synchronous.
    /// </summary>
    public string InvokeFunctionActionNameSync { get; init; } = INVOKE_FUNCTION_ACTION_NAME_SYNC;
    private const string INVOKE_FUNCTION_ACTION_NAME_SYNC = "Invoke";
    
    /// <summary>
    /// Naming of the #action# variable for the invoke module functions name pattern when the action is try synchronous.
    /// </summary>
    public string InvokeFunctionActionNameTrySync { get; init; } = INVOKE_FUNCTION_ACTION_NAME_TRYSYNC;
    private const string INVOKE_FUNCTION_ACTION_NAME_TRYSYNC = "InvokeTrySync";
    
    /// <summary>
    /// Naming of the #action# variable for the invoke module functions name pattern when the action is asynchronous.
    /// </summary>
    public string InvokeFunctionActionNameAsync { get; init; } = INVOKE_FUNCTION_ACTION_NAME_ASYNC;
    private const string INVOKE_FUNCTION_ACTION_NAME_ASYNC = "InvokeAsync";


    /// <summary>
    /// <para>If true, whenever a module function returns a promise, the <see cref="InvokeFunctionSyncEnabled" />, <see cref="InvokeFunctionTrySyncEnabled" /> and <see cref="InvokeFunctionAsyncEnabled" /> flags will be ignored<br />
    /// and instead only the async invoke method will be generated.</para>
    /// <para>This value should always be true. Set it only to false when you know what you are doing.</para>
    /// </summary>
    public bool PromiseOnlyAsync { get; init; } = PROMISE_ONLY_ASYNC;
    private const bool PROMISE_ONLY_ASYNC = true;
    
    /// <summary>
    /// <para>If true, whenever a module function returns a promise, the string "Async" is appended.</para>
    /// <para>If your pattern ends already with "Async", for example with the #action# variable, this will result in a double: "AsyncAsync"</para>
    /// </summary>
    public bool PromiseAppendAsync { get; init; } = PROMISE_APPEND_ASYNC;
    private const bool PROMISE_APPEND_ASYNC = false;


    /// <summary>
    /// <para>Mapping of typescript-types (key) to C#-types (value).</para>
    /// <para>Not listed types are mapped unchanged (Identity function).</para>
    /// </summary>
    public Dictionary<string, string> TypeMap { get; init; } = TypeMapDefault;
    private static Dictionary<string, string> TypeMapDefault => new() {
        ["number"] = "double",
        ["boolean"] = "bool",
        ["Uint8Array"] = "byte[]",
        ["HTMLObjectElement"] = "ElementReference"
    };

    #endregion


    #region preload

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

    #endregion


    #region js runtime

    /// <summary>
    /// Toggles whether generic JSRuntime sync invoke method should be generated.
    /// </summary>
    public bool JSRuntimeSyncEnabled { get; init; } = JSRUNTIME_SYNC_ENABLED;
    private const bool JSRUNTIME_SYNC_ENABLED = false;
    
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

    #endregion


    /// <summary>
    /// Creates a <see cref="Config"/> instance with default values.
    /// </summary>
    public Config() { }

    /// <summary>
    /// Creates a <see cref="Config"/> instance with the given json.
    /// </summary>
    public Config(string json) {
        JsonNode root = JsonNode.Parse(json) ?? throw new ArgumentException($"json is not in a valid format:\n{json}");


        // DeclarationPath
        {
            try {
                DeclarationPath = root["declaration path"] switch {
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


        // FileOutputClass, FileOutputinterface;
        {
            if (root.AsJsonObjectOrNull("file output") is JsonObject jsonObject) {
                FileOutputClass = jsonObject["class"]?.ParseAsString("[file output].class") ?? FILE_OUTPUT_CLASS;
                FileOutputinterface = jsonObject["interface"]?.ParseAsString("[file output].interface") ?? FILE_OUTPUT_INTERFACE;
            }
            else {
                FileOutputClass = FILE_OUTPUT_CLASS;
                FileOutputinterface = FILE_OUTPUT_INTERFACE;
            }
        }

        GenerateOnSave = root["generate on save"]?.ParseAsBool("generate on save") ?? GENERATE_ON_SAVE;

        UsingStatements = root.ParseAsStringArray("using statements") ?? new string[1] { USING_STATEMENT };


        // InvokeFunctionSyncEnabled,InvokeFunctionTrySyncEnabled,InvokeFunctionAsyncEnabled ,InvokeFunctionNamePattern ,PromiseOnlyAsync,PromiseAppendAsync ,TypeMap
        {
            if (root.AsJsonObjectOrNull("invoke function") is JsonObject jsonObject) {
                InvokeFunctionSyncEnabled = jsonObject["sync enabled"]?.ParseAsBool("[invoke function].[sync enabled]") ?? INVOKE_FUNCTION_SYNC_ENABLED;
                InvokeFunctionTrySyncEnabled = jsonObject["trysync enabled"]?.ParseAsBool("[invoke function].[trysync enabled]") ?? INVOKE_FUNCTION_TRYSYNC_ENABLED;
                InvokeFunctionAsyncEnabled = jsonObject["async enabled"]?.ParseAsBool("[invoke function].[async enabled]") ?? INVOKE_FUNCTION_ASYNC_ENABLED;

                if (jsonObject.AsJsonObjectOrNull("name pattern", "[invoke function],[name pattern]") is JsonObject namePatternJsonObject) {
                    string namePattern = namePatternJsonObject["pattern"]?.ParseAsString("[invoke function].[name pattern].pattern") ?? INVOKE_FUNCTION_NAME_PATTERN;
                    NameTransform moduleTransform = namePatternJsonObject["module transform"]?.ParseAsNameTransform("[invoke function],[name pattern].[module transform]") ?? INVOKE_FUNCTION_MODULE_TRANSFORM;
                    NameTransform functionTransform = namePatternJsonObject["function transform"]?.ParseAsNameTransform("[invoke function],[name pattern].[function transform]") ?? INVOKE_FUNCTION_FUNCTION_TRANSFORM;
                    NameTransform actionTransform = namePatternJsonObject["action transform"]?.ParseAsNameTransform("[invoke function],[name pattern].[action transform]") ?? INVOKE_FUNCTION_ACTION_TRANSFORM;
                    InvokeFunctionNamePattern = new FunctionNamePattern(namePattern, moduleTransform, functionTransform, actionTransform);

                    if (namePatternJsonObject.AsJsonObjectOrNull("action name", "[invoke function],[name pattern].[action name]") is JsonObject actionNameJsonObject) {
                        InvokeFunctionActionNameSync = actionNameJsonObject["sync"]?.ParseAsString("[invoke function].[name pattern].sync") ?? INVOKE_FUNCTION_ACTION_NAME_SYNC;
                        InvokeFunctionActionNameTrySync = actionNameJsonObject["trysync"]?.ParseAsString("[invoke function].[name pattern].trysync") ?? INVOKE_FUNCTION_ACTION_NAME_TRYSYNC;
                        InvokeFunctionActionNameAsync = actionNameJsonObject["async"]?.ParseAsString("[invoke function].[name pattern].async") ?? INVOKE_FUNCTION_ACTION_NAME_ASYNC;
                    }
                    else {
                        InvokeFunctionActionNameSync = INVOKE_FUNCTION_ACTION_NAME_SYNC;
                        InvokeFunctionActionNameTrySync = INVOKE_FUNCTION_ACTION_NAME_TRYSYNC;
                        InvokeFunctionActionNameAsync = INVOKE_FUNCTION_ACTION_NAME_ASYNC;
                    }
                }
                else {
                    InvokeFunctionNamePattern = new FunctionNamePattern(INVOKE_FUNCTION_NAME_PATTERN, INVOKE_FUNCTION_MODULE_TRANSFORM, INVOKE_FUNCTION_FUNCTION_TRANSFORM, INVOKE_FUNCTION_ACTION_TRANSFORM);

                    InvokeFunctionActionNameSync = INVOKE_FUNCTION_ACTION_NAME_SYNC;
                    InvokeFunctionActionNameTrySync = INVOKE_FUNCTION_ACTION_NAME_TRYSYNC;
                    InvokeFunctionActionNameAsync = INVOKE_FUNCTION_ACTION_NAME_ASYNC;
                }

                if (jsonObject.AsJsonObjectOrNull("promise") is JsonObject promiseJsonObject) {
                    PromiseOnlyAsync = promiseJsonObject["only async enabled"]?.ParseAsBool("[invoke function].[promise].[only async enabled]") ?? PROMISE_ONLY_ASYNC;
                    PromiseAppendAsync = promiseJsonObject["append Async"]?.ParseAsBool("[invoke function].[promise].[append Async]") ?? PROMISE_APPEND_ASYNC;
                }
                else {
                    PromiseOnlyAsync = PROMISE_ONLY_ASYNC;
                    PromiseAppendAsync = PROMISE_APPEND_ASYNC;
                }

                TypeMap = jsonObject.ParseAsStringDictionary("type map") ?? TypeMapDefault;
            }
            else {
                InvokeFunctionSyncEnabled = INVOKE_FUNCTION_SYNC_ENABLED;
                InvokeFunctionTrySyncEnabled = INVOKE_FUNCTION_TRYSYNC_ENABLED;
                InvokeFunctionAsyncEnabled = INVOKE_FUNCTION_ASYNC_ENABLED;

                InvokeFunctionNamePattern = new FunctionNamePattern(INVOKE_FUNCTION_NAME_PATTERN, INVOKE_FUNCTION_MODULE_TRANSFORM, INVOKE_FUNCTION_FUNCTION_TRANSFORM, INVOKE_FUNCTION_ACTION_TRANSFORM);

                InvokeFunctionActionNameSync = INVOKE_FUNCTION_ACTION_NAME_SYNC;
                InvokeFunctionActionNameTrySync = INVOKE_FUNCTION_ACTION_NAME_TRYSYNC;
                InvokeFunctionActionNameAsync = INVOKE_FUNCTION_ACTION_NAME_ASYNC;
                
                PromiseOnlyAsync = PROMISE_ONLY_ASYNC;
                PromiseAppendAsync = PROMISE_APPEND_ASYNC;

                TypeMap = TypeMapDefault;
            }
        }


        // PreloadNamePattern, PreloadAllModulesName
        {
            if (root.AsJsonObjectOrNull("preload function") is JsonObject jsonObject) {
                if (jsonObject.AsJsonObjectOrNull("name pattern", "[preload function],[name pattern]") is JsonObject namePatternJsonObject) {
                    string namePattern = namePatternJsonObject["pattern"]?.ParseAsString("[preload function],[name pattern].pattern") ?? PRELOAD_NAME_PATTERN;
                    NameTransform moduleTransform = namePatternJsonObject["module transform"]?.ParseAsNameTransform("[preload function],[name pattern].[module transform]") ?? PRELOAD_MODULE_TRANSFORM;
                    PreloadNamePattern = new ModuleNamePattern(namePattern, moduleTransform);
                }
                else
                    PreloadNamePattern = new ModuleNamePattern(PRELOAD_NAME_PATTERN, PRELOAD_MODULE_TRANSFORM);

                PreloadAllModulesName = jsonObject["all modules name"]?.ParseAsString("[preload function],[all modules name]") ?? PRELOAD_ALL_MODULES_NAME;
            }
            else {
                PreloadNamePattern = new ModuleNamePattern(PRELOAD_NAME_PATTERN, PRELOAD_MODULE_TRANSFORM);

                PreloadAllModulesName = PRELOAD_ALL_MODULES_NAME;
            }
        }


        // JSRuntimeSyncEnabled, JSRuntimeTrySyncEnabled, JSRuntimeAsyncEnabled
        {
            if (root.AsJsonObjectOrNull("js runtime") is JsonObject jsonObject) {
                JSRuntimeSyncEnabled = jsonObject["sync enabled"]?.ParseAsBool("[js runtime].[sync enabled]") ?? JSRUNTIME_SYNC_ENABLED;
                JSRuntimeTrySyncEnabled = jsonObject["trysync enabled"]?.ParseAsBool("[js runtime].[trysync enabled]") ?? JSRUNTIME_TRYSYNC_ENABLED;
                JSRuntimeAsyncEnabled = jsonObject["async enabled"]?.ParseAsBool("[js runtime].[async enabled]") ?? JSRUNTIME_ASYNC_ENABLED;
            }
            else {
                JSRuntimeSyncEnabled = JSRUNTIME_SYNC_ENABLED;
                JSRuntimeTrySyncEnabled = JSRUNTIME_TRYSYNC_ENABLED;
                JSRuntimeAsyncEnabled = JSRUNTIME_ASYNC_ENABLED;
            }
        }
    }

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
              "generate on save": {{(GenerateOnSave ? "true" : "false")}},
              "using statements": [{{usingStatements}}],
              "invoke function": {
                "sync enabled": {{(InvokeFunctionSyncEnabled ? "true" : "false")}},
                "trysync enabled": {{(InvokeFunctionTrySyncEnabled ? "true" : "false")}},
                "async enabled": {{(InvokeFunctionAsyncEnabled ? "true" : "false")}},
                "name pattern": {
                  "pattern": "{{InvokeFunctionNamePattern.NamePattern}}",
                  "module transform": "{{InvokeFunctionNamePattern.ModuleTransform}}",
                  "function transform": "{{InvokeFunctionNamePattern.FunctionTransform}}",
                  "action transform": "{{InvokeFunctionNamePattern.ActionTransform}}",
                  "action name": {
                    "sync": "{{InvokeFunctionActionNameSync}}",
                    "trysync": "{{InvokeFunctionActionNameTrySync}}",
                    "async": "{{InvokeFunctionActionNameAsync}}"
                  }
                },
                "promise": {
                  "only async enabled": {{(PromiseOnlyAsync ? "true" : "false")}},
                  "append Async": {{(PromiseAppendAsync ? "true" : "false")}}
                },
                "type map": {{{typeMap}}}
              },
              "preload function": {
                "name pattern": {
                  "pattern": "{{PreloadNamePattern.NamePattern}}",
                  "module transform": "{{PreloadNamePattern.ModuleTransform}}"
                },
                "all modules name": "{{PRELOAD_ALL_MODULES_NAME}}"
              },
              "js runtime": {
                "sync enabled": {{(JSRuntimeSyncEnabled ? "true" : "false")}},
                "trysync enabled": {{(JSRuntimeTrySyncEnabled ? "true" : "false")}},
                "async enabled": {{(JSRuntimeAsyncEnabled ? "true" : "false")}}
              }
            }

            """;
    }

    
    /// <summary>
    /// Compares all values when changed result in a change of the structureTree.
    /// </summary>
    /// <param name="other"></param>
    /// <remarks><see cref="DeclarationPath"/> is not included here, although it changes the structureTree, because it also changes parsing paths and therefore must be treated especially.</remarks>
    /// <returns>true, when all values are the same and thereby no change in the structureTree happened.</returns>
    public bool StructureTreeEquals(Config other) {
        if (!UsingStatementsEqual(this, other))
            return false;

        if (InvokeFunctionSyncEnabled != other.InvokeFunctionSyncEnabled)
            return false;
        if (InvokeFunctionTrySyncEnabled != other.InvokeFunctionTrySyncEnabled)
            return false;
        if (InvokeFunctionAsyncEnabled != other.InvokeFunctionAsyncEnabled)
            return false;

        if (InvokeFunctionNamePattern != other.InvokeFunctionNamePattern)
            return false;

        if (InvokeFunctionActionNameSync != other.InvokeFunctionActionNameSync)
            return false;
        if (InvokeFunctionActionNameTrySync != other.InvokeFunctionActionNameTrySync)
            return false;
        if (InvokeFunctionActionNameAsync != other.InvokeFunctionActionNameAsync)
            return false;

        if (PromiseOnlyAsync != other.PromiseOnlyAsync)
            return false;
        if (PromiseAppendAsync != other.PromiseAppendAsync)
            return false;

        if (!TypeMapEqual(this, other))
            return false;
        
        if (PreloadNamePattern != other.PreloadNamePattern)
            return false;
        if (PreloadAllModulesName != other.PreloadAllModulesName)
            return false;

        if (JSRuntimeSyncEnabled != other.JSRuntimeSyncEnabled)
            return false;
        if (JSRuntimeTrySyncEnabled != other.JSRuntimeTrySyncEnabled)
            return false;
        if (JSRuntimeAsyncEnabled != other.JSRuntimeAsyncEnabled)
            return false;
        
        return true;


        static bool UsingStatementsEqual(Config a, Config b) {
            if (a.UsingStatements.Length != b.UsingStatements.Length)
                return false;

            for (int i = 0; i < a.UsingStatements.Length; i++)
                if (a.UsingStatements[i] != b.UsingStatements[i])
                    return false;

            return true;
        }

        static bool TypeMapEqual(Config a, Config b) {
            if (a.TypeMap.Count != b.TypeMap.Count)
                return false;

            foreach (KeyValuePair<string, string> pair in a.TypeMap) {
                if (!b.TypeMap.TryGetValue(pair.Key, out string value))
                    return false;

                if (pair.Value != value)
                    return false;
            }

            return true;
        }
    }
}

file static class JsonNodeExtension {
    internal static JsonObject? AsJsonObjectOrNull(this JsonNode parentNode, string key, string? parentkey = null)
        => parentNode[key] switch {
            JsonObject jsonObject => jsonObject,
            null => null,
            _ => throw JsonException.UnexpectedType(parentkey ?? key)
        };
    
    internal static string[]? ParseAsStringArray(this JsonNode parentNode, string parentKey)
        => parentNode[parentKey] switch {
            JsonArray array => array.ParseArrayAsStrings(parentKey),
            JsonValue valueNode => new string[1] { valueNode.ParseAsString(parentKey) },
            null => null,
            _ => throw JsonException.UnexpectedType(parentKey)
        };

    internal static string[] ParseArrayAsStrings(this JsonArray array, string parentKey) {
        string[] result = new string[array.Count];

        for (int i = 0; i < array.Count; i++)
            try {
                result[i] = array[i].ParseAsString(parentKey);
            }
            catch (ArgumentException exception) { throw new ArgumentException($"{exception.Message}, at array index {i}", exception); }

        return result;
    }


    internal static Dictionary<string, string>? ParseAsStringDictionary(this JsonNode parentNode, string parentKey)
        => parentNode[parentKey] switch {
            JsonObject jsonObject => jsonObject.ParseObjectAsStringDictionary(parentKey),
            null => null,
            _ => throw JsonException.UnexpectedType(parentKey)
        };
    
    internal static Dictionary<string, string> ParseObjectAsStringDictionary(this JsonObject jsonObject, string parentKey) {
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
