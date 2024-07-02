using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;
using TSRuntime.Configs.NamePattern;

namespace TSRuntime.Configs;

/// <summary>
/// The configurations for generating the ITSRuntime content.
/// </summary>
public sealed class Config : IEquatable<Config> {
    /// <summary>
    /// Absolute path to the web root.
    /// </summary>
    public string WebRootPath { get; init; } = string.Empty;

    /// <summary>
    /// Declares the input source. It contains folder and file paths where each paths can have some more properties.
    /// </summary>
    public InputPath[] InputPath { get; init; } = InputPathDefault;
    private static readonly InputPath[] InputPathDefault = [new InputPath("/", ["/bin", "/obj", "/Properties"])];


    /// <summary>
    /// List of generated using statements at the top of ITSRuntime.
    /// </summary>
    public string[] UsingStatements { get; init; } = UsingStatementsDefault;
    private static string[] UsingStatementsDefault => [ "Microsoft.AspNetCore.Components", "System.Numerics" ];


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
    public FunctionNamePattern InvokeFunctionNamePattern { get; init; } = new(INVOKE_FUNCTION_NAME_PATTERN, INVOKE_FUNCTION_MODULE_TRANSFORM, INVOKE_FUNCTION_FUNCTION_TRANSFORM, INVOKE_FUNCTION_ACTION_TRANSFORM, null!);
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
    /// <para>Whenever a module function returns a promise, the <see cref="InvokeFunctionSyncEnabled" />, <see cref="InvokeFunctionTrySyncEnabled" /> and <see cref="InvokeFunctionAsyncEnabled" /> flags will be ignored<br />
    /// and instead only the async invoke method will be generated.</para>
    /// <para>This value should always be true. Set it only to false when you know what you are doing.</para>
    /// </summary>
    public bool PromiseOnlyAsync { get; init; } = PROMISE_ONLY_ASYNC;
    private const bool PROMISE_ONLY_ASYNC = true;
    
    /// <summary>
    /// <para>Whenever a module function returns a promise, the string "Async" is appended.</para>
    /// <para>If your pattern ends already with "Async", for example with the #action# variable, this will result in a double: "AsyncAsync"</para>
    /// </summary>
    public bool PromiseAppendAsync { get; init; } = PROMISE_APPEND_ASYNC;
    private const bool PROMISE_APPEND_ASYNC = false;


    /// <summary>
    /// <para>Mapping of typescript-types (key) to C#-types (value).</para>
    /// <para>Not listed types are mapped unchanged (Identity function).</para>
    /// </summary>
    public Dictionary<string, MappedType> TypeMap { get; init; } = TypeMapDefault;
    private static Dictionary<string, MappedType> TypeMapDefault => new() {
        ["number"] = new MappedType("TNumber", new GenericType("TNumber") { Constraint = "INumber<TNumber>" }),
        ["boolean"] = new MappedType("bool"),
        ["Uint8Array"] = new MappedType("byte[]"),
        ["HTMLElement"] = new MappedType("ElementReference")
    };

    #endregion


    #region preload

    /// <summary>
    /// Naming of the generated methods that preloads a specific module.
    /// </summary>
    public ModuleNamePattern PreloadNamePattern { get; init; } = new(PRELOAD_NAME_PATTERN, PRELOAD_MODULE_TRANSFORM, null!);
    private const string PRELOAD_NAME_PATTERN = "Preload#module#";
    private const NameTransform PRELOAD_MODULE_TRANSFORM = NameTransform.FirstUpperCase;

    /// <summary>
    /// Naming of the method that preloads all modules.
    /// </summary>
    public string PreloadAllModulesName { get; init; } = PRELOAD_ALL_MODULES_NAME;
    private const string PRELOAD_ALL_MODULES_NAME = "PreloadAllModules";

    #endregion


    #region module grouping

    /// <summary>
    /// Each module gets it own interface and the functions of that module are only available in that interface.
    /// </summary>
    public bool ModuleGrouping { get; init; } = MODULE_GROUPING;
    private const bool MODULE_GROUPING = false;

    /// <summary>
    /// Naming of the generated module interfaces when <see cref="ModuleGrouping"/> is enabled.
    /// </summary>
    public ModuleNamePattern ModuleGroupingNamePattern { get; init; } = new(MODULE_GROUPING_NAME_PATTERN, MODULE_GROUPING_MODULE_TRANSFORM, null!);
    private const string MODULE_GROUPING_NAME_PATTERN = "I#module#Module";
    private const NameTransform MODULE_GROUPING_MODULE_TRANSFORM = NameTransform.FirstUpperCase;

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
    /// A service extension method is generated, which registers ITSRuntime and if available, the module interfaces.
    /// </summary>
    public bool ServiceExtension { get; init; } = SERVICE_EXTENSION;
    private const bool SERVICE_EXTENSION = true;


    /// <summary>
    /// Diagnostics, containing warnings and errors.
    /// </summary>
    public List<Diagnostic> ErrorList { get; } = [];


    /// <summary>
    /// Creates a <see cref="Config"/> instance with default values.
    /// </summary>
    public Config() { }

    /// <summary>
    /// Creates a <see cref="Config"/> instance with the given json.
    /// </summary>
    /// <param name="filePath">the root path for the paths in <see cref="InputPath"/>.</param>
    /// <param name="json"></param>
    /// <exception cref="ArgumentException"></exception>
    [SetsRequiredMembers]
    public Config(string filePath, string json) {
        JsonNode? root = JsonNode.Parse(json);
        if (root is null) {
            ErrorList.AddConfigInvalidError();
            return;
        }

        // WebRootPath
        string relativePath = root.ParseValue("webroot path", ErrorList, "[webroot path]", string.Empty);
        string absolutePathNotEvaluated = Path.Combine(filePath, relativePath);
        string absolutePath = Path.GetFullPath(absolutePathNotEvaluated);
        WebRootPath = Normalize(absolutePath);


        // InputPath
        switch (root["input path"]) {
            case JsonArray jsonArray: {
                InputPath = jsonArray.Select((JsonNode? node, int index) => {
                    switch (node) {
                        case JsonObject jsonObject: {
                            string include;
                            switch (jsonObject["include"]) {
                                case JsonValue jsonValue: {
                                    string? str = jsonValue.ParseValue<string?>(ErrorList, $"[input path (index {index})].[include]", null);
                                    if (str is null)
                                        return null;
                                    include = Normalize(str);
                                    if (include is not ['/', ..] and { Length: > 0 })
                                        ErrorList.AddInputPathNoStartingSlashError($"[input path (index {index})].[include]");
                                    break;
                                }
                                case null: {
                                    ErrorList.AddConfigKeyNotFoundError($"[input path (index {index})].[include]");
                                    return null;
                                }
                                case not null: {
                                    ErrorList.AddConfigUnexpectedTypeError($"[input path (index {index})].[include]");
                                    return null;
                                }
                            }

                            string[] excludes;
                            switch (jsonObject["excludes"]) {
                                case JsonArray jsonArray: {
                                    excludes = jsonArray.Select((JsonNode? node, int excludeIndex) => {
                                        string? result = node.ParseValue<string?>(ErrorList, $"[input path (index {index})].[excludes (index {excludeIndex})]", null);
                                        if (result is not null) {
                                            result = Normalize(result);
                                            if (result is not ['/', ..] and { Length: > 0 })
                                                ErrorList.AddInputPathNoStartingSlashError($"[input path (index {index})].[excludes (index {excludeIndex})]");
                                        }
                                        return result;
                                    });
                                    break;
                                }
                                case JsonValue jsonValue: {
                                    if (jsonValue.TryGetValue(out string? str)) {
                                        str = Normalize(str);
                                        if (str is not ['/', ..] and { Length: > 0 })
                                            ErrorList.AddInputPathNoStartingSlashError($"[input path (index {index})].[excludes]");
                                        excludes = [str];
                                    }
                                    else {
                                        ErrorList.AddConfigStringExpectedError($"[input path (index {index})].[excludes]");
                                        excludes = [];
                                    }
                                    break;
                                }
                                case not null: {
                                    ErrorList.AddConfigUnexpectedTypeError($"[input path (index {index})].[excludes]");
                                    goto case null;
                                }
                                case null: {
                                    excludes = [];
                                    break;
                                }
                            }

                            bool moduleFiles;
                            switch (jsonObject["module files"]) {
                                case JsonValue jsonValue: {
                                    moduleFiles = jsonValue.ParseValue(ErrorList, $"[input path (index {index})].[module files]", true);
                                    break;
                                }
                                case not null: {
                                    ErrorList.AddConfigUnexpectedTypeError($"[input path (index {index})].[module files]");
                                    goto case null;
                                }
                                case null: {
                                    moduleFiles = true;
                                    break;
                                }
                            }

                            string? modulePath;
                            switch (jsonObject["module path"]) {
                                case JsonValue jsonValue: {
                                    modulePath = jsonValue.ParseValue<string?>(ErrorList, $"[input path (index {index})].[module path]", null);
                                    if (modulePath is not null) {
                                        modulePath = Normalize(modulePath);
                                        if (modulePath is not ['/', ..] and { Length: > 0 })
                                            ErrorList.AddInputPathNoStartingSlashError($"[input path (index {index})].[module path]");
                                        if (modulePath is not [.., '.', 'j', 's'])
                                            ErrorList.AddModulePathNoJsExtensionError($"[input path (index {index})].[module path]");
                                    }
                                    break;
                                }
                                case not null: {
                                    ErrorList.AddConfigUnexpectedTypeError($"[input path (index {index})].[module path]");
                                    goto case null;
                                }
                                case null: {
                                    modulePath = null;
                                    break;
                                }
                            }

                            return new InputPath(include, excludes, moduleFiles, modulePath);
                        }
                        case JsonValue jsonValue: {
                            if (jsonValue.TryGetValue(out string? include)) {
                                include = Normalize(include);
                                if (include is not ['/', ..] and { Length: > 0 })
                                    ErrorList.AddInputPathNoStartingSlashError($"[input path (index {index})]");
                                return new InputPath(include);
                            }
                            else {
                                ErrorList.AddConfigStringExpectedError($"[input path (index {index})]");
                                return null;
                            }
                        }
                        default: {
                            ErrorList.AddConfigUnexpectedTypeError($"[input path (index {index})]");
                            return null;
                        }
                    }
                });
                break;
            }
            case JsonObject jsonObject: {
                string include;
                switch (jsonObject["include"]) {
                    case JsonValue jsonValue: {
                        include = Normalize(jsonValue.ParseValue(ErrorList, "[input path].[include]", string.Empty));
                        if (include is not ['/', ..] and { Length: > 0 })
                            ErrorList.AddInputPathNoStartingSlashError("[input path].[include]");
                        break;
                    }
                    case null: {
                        ErrorList.AddConfigKeyNotFoundError("[input path].[include]");
                        include = string.Empty;
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError("[input path].[include]");
                        include = string.Empty;
                        break;
                    }
                }

                string[] excludes;
                switch (jsonObject["excludes"]) {
                    case JsonArray jsonArray: {
                        excludes = jsonArray.Select((JsonNode? node, int index) => {
                            string? result = node.ParseValue<string?>(ErrorList, $"[input path].[excludes (index {index})]", null);
                            if (result is not null) {
                                result = Normalize(result);
                                if (result is not ['/', ..] and { Length: > 0 })
                                    ErrorList.AddInputPathNoStartingSlashError($"[input path].[excludes (index {index})]");
                            }
                            return result;
                        });
                        break;
                    }
                    case JsonValue jsonValue: {
                        if (jsonValue.TryGetValue(out string? value)) {
                            value = Normalize(value);
                            if (value is not ['/', ..] and { Length: > 0 })
                                ErrorList.AddInputPathNoStartingSlashError("[input path].[excludes]");
                            excludes = [value];
                        }
                        else {
                            ErrorList.AddConfigStringExpectedError("[input path].[excludes]");
                            excludes = [];
                        }
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError("[input path].[excludes]");
                        goto case null;
                    }
                    case null: {
                        excludes = [];
                        break;
                    }
                }

                bool moduleFiles;
                switch (jsonObject["module files"]) {
                    case JsonValue jsonValue: {
                        moduleFiles = jsonValue.ParseValue(ErrorList, $"[input path].[module files]", true);
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError($"[input path].[module files]");
                        goto case null;
                    }
                    case null: {
                        moduleFiles = true;
                        break;
                    }
                }

                string? modulePath;
                switch (jsonObject["module path"]) {
                    case JsonValue jsonValue: {
                        modulePath = jsonValue.ParseValue<string?>(ErrorList, "[input path].[module path]", null);
                        if (modulePath is not null) {
                            modulePath = Normalize(modulePath);
                            if (modulePath is not ['/', ..] and { Length: > 0 })
                                ErrorList.AddInputPathNoStartingSlashError("[input path].[module path]");
                            if (modulePath is not [.., '.', 'j', 's'])
                                ErrorList.AddModulePathNoJsExtensionError("[input path].[module path]");
                        }
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError("[input path].[module path]");
                        goto case null;
                    }
                    case null: {
                        modulePath = null;
                        break;
                    }
                }

                InputPath = [new InputPath(include, excludes, moduleFiles, modulePath)];
                break;
            }
            case JsonValue jsonValue: {
                if (jsonValue.TryGetValue(out string? include)) {
                    include = Normalize(include);
                    if (include is not ['/', ..] and { Length: > 0 })
                        ErrorList.AddInputPathNoStartingSlashError("[input path]");
                    InputPath = [new InputPath(include)];
                }
                else {
                    ErrorList.AddConfigStringExpectedError("[input path]");
                    InputPath = InputPathDefault;
                }
                break;
            }
            case not null: {
                ErrorList.AddConfigUnexpectedTypeError("[input path]");
                goto case null;
            }
            case null: {
                InputPath = InputPathDefault;
                break;
            }
        }


        // UsingStatements
        switch (root["using statements"]) {
            case JsonArray array: {
                UsingStatements = array.Select((JsonNode? node, int index) => node.ParseValue<string?>(ErrorList, $"[using statements (index {index})]", null));
                break;
            }
            case JsonValue jsonValue: {
                if (jsonValue.TryGetValue(out string? value))
                    UsingStatements = [value];
                else {
                    ErrorList.AddConfigStringExpectedError("using statements");
                    UsingStatements = UsingStatementsDefault;
                }
                break;
            }
            case not null: {
                ErrorList.AddConfigUnexpectedTypeError("[using statements]");
                goto case null;
            }
            case null: {
                UsingStatements = UsingStatementsDefault;
                break;
            }
        }


        // InvokeFunctionSyncEnabled, InvokeFunctionTrySyncEnabled, InvokeFunctionAsyncEnabled
        // InvokeFunctionNamePattern
        // InvokeFunctionActionNameSync, InvokeFunctionActionNameTrySync, InvokeFunctionActionNameAsync
        // PromiseOnlyAsync, PromiseAppendAsync
        // TypeMap
        switch (root["invoke function"]) {
            case JsonObject jsonObject: {
                InvokeFunctionSyncEnabled = jsonObject.ParseValue("sync enabled", ErrorList, "[invoke function].[sync enabled]", INVOKE_FUNCTION_SYNC_ENABLED);
                InvokeFunctionTrySyncEnabled = jsonObject.ParseValue("trysync enabled", ErrorList, "[invoke function].[trysync enabled]", INVOKE_FUNCTION_TRYSYNC_ENABLED);
                InvokeFunctionAsyncEnabled = jsonObject.ParseValue("async enabled", ErrorList, "[invoke function].[async enabled]", INVOKE_FUNCTION_ASYNC_ENABLED);

                switch (jsonObject["name pattern"]) {
                    case JsonObject namePatternJsonObject: {
                        string namePattern = namePatternJsonObject.ParseValue("pattern", ErrorList, "[invoke function].[name pattern].[pattern]", INVOKE_FUNCTION_NAME_PATTERN);
                        NameTransform moduleTransform = namePatternJsonObject.ParseNameTransform("module transform", ErrorList, "[invoke function].[name pattern].[module transform]", INVOKE_FUNCTION_MODULE_TRANSFORM);
                        NameTransform functionTransform = namePatternJsonObject.ParseNameTransform("function transform", ErrorList, "[invoke function].[name pattern].[function transform]", INVOKE_FUNCTION_FUNCTION_TRANSFORM);
                        NameTransform actionTransform = namePatternJsonObject.ParseNameTransform("action transform", ErrorList, "[invoke function].[name pattern].[action transform]", INVOKE_FUNCTION_ACTION_TRANSFORM);
                        InvokeFunctionNamePattern = new FunctionNamePattern(namePattern, moduleTransform, functionTransform, actionTransform, ErrorList);

                        switch (namePatternJsonObject["action name"]) {
                            case JsonObject actionNameJsonObject: {
                                InvokeFunctionActionNameSync = actionNameJsonObject.ParseValue("sync", ErrorList, "[invoke function].[name pattern].[sync]", INVOKE_FUNCTION_ACTION_NAME_SYNC);
                                InvokeFunctionActionNameTrySync = actionNameJsonObject.ParseValue("trysync", ErrorList, "[invoke function].[name pattern].[trysync]", INVOKE_FUNCTION_ACTION_NAME_TRYSYNC);
                                InvokeFunctionActionNameAsync = actionNameJsonObject.ParseValue("async", ErrorList, "[invoke function].[name pattern].[async]", INVOKE_FUNCTION_ACTION_NAME_ASYNC);
                                break;
                            }
                            case not null: {
                                ErrorList.AddConfigUnexpectedTypeError("[invoke function].[name pattern].[action name]");
                                goto case null;
                            }
                            case null: {
                                InvokeFunctionActionNameSync = INVOKE_FUNCTION_ACTION_NAME_SYNC;
                                InvokeFunctionActionNameTrySync = INVOKE_FUNCTION_ACTION_NAME_TRYSYNC;
                                InvokeFunctionActionNameAsync = INVOKE_FUNCTION_ACTION_NAME_ASYNC;
                                break;
                            }
                        }
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError("[invoke function].[name pattern]");
                        goto case null;
                    }
                    case null: {
                        InvokeFunctionNamePattern = new FunctionNamePattern(INVOKE_FUNCTION_NAME_PATTERN, INVOKE_FUNCTION_MODULE_TRANSFORM, INVOKE_FUNCTION_FUNCTION_TRANSFORM, INVOKE_FUNCTION_ACTION_TRANSFORM, null!);

                        InvokeFunctionActionNameSync = INVOKE_FUNCTION_ACTION_NAME_SYNC;
                        InvokeFunctionActionNameTrySync = INVOKE_FUNCTION_ACTION_NAME_TRYSYNC;
                        InvokeFunctionActionNameAsync = INVOKE_FUNCTION_ACTION_NAME_ASYNC;
                        break;
                    }
                }
                // 'function transform should contain "#action#"'-check
                int methodEnabledCount = 0;
                if (InvokeFunctionSyncEnabled)
                    methodEnabledCount++;
                if (InvokeFunctionTrySyncEnabled)
                    methodEnabledCount++;
                if (InvokeFunctionAsyncEnabled)
                    methodEnabledCount++;
                if (methodEnabledCount >= 2 && !InvokeFunctionNamePattern.NamePattern.Contains("#action#"))
                    ErrorList.AddConfigFunctionTransformMissingActionError("[invoke function].[name pattern].[pattern]");

                switch (jsonObject["promise"]) {
                    case JsonObject promiseJsonObject: {
                        PromiseOnlyAsync = promiseJsonObject.ParseValue("only async enabled", ErrorList, "[invoke function].[promise].[only async enabled]", PROMISE_ONLY_ASYNC);
                        PromiseAppendAsync = promiseJsonObject.ParseValue("append async", ErrorList, "[invoke function].[promise].[append async]", PROMISE_APPEND_ASYNC);
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError("[invoke function].[promise]");
                        goto case null;
                    }
                    case null: {
                        PromiseOnlyAsync = PROMISE_ONLY_ASYNC;
                        PromiseAppendAsync = PROMISE_APPEND_ASYNC;
                        break;
                    }
                }

                // TypeMap
                switch (jsonObject["type map"]) {
                    case JsonObject typeMapJsonObject: {
                        TypeMap = new Dictionary<string, MappedType>(typeMapJsonObject.Count);

                        foreach (KeyValuePair<string, JsonNode?> item in typeMapJsonObject) {
                            switch (typeMapJsonObject[item.Key]) {
                                case JsonValue mapJsonValue: {
                                    string? value = mapJsonValue.ParseValue<string?>(ErrorList, $"[invoke function].[type map].[key ({item.Key})]", null);
                                    if (value is null)
                                        break;

                                    TypeMap.Add(item.Key, new MappedType(value));
                                    break;
                                }
                                case JsonObject mapJsonObject: {
                                    string type;
                                    switch (mapJsonObject["type"]) {
                                        case JsonValue typeValue: {
                                            string? typeString = typeValue.ParseValue<string?>(ErrorList, $"[invoke function].[type map].[key ({item.Key})].[type]", null);
                                            if (typeString is null)
                                                continue;

                                            type = typeString;
                                            break;
                                        }
                                        case null: {
                                            ErrorList.AddConfigKeyNotFoundError($"[invoke function].[type map].[key ({item.Key})].[type]");
                                            continue;
                                        }
                                        case not null: {
                                            ErrorList.AddConfigUnexpectedTypeError($"[invoke function].[type map].[key ({item.Key})].[type]");
                                            continue;
                                        }
                                    }

                                    GenericType[] genericTypes;
                                    switch (mapJsonObject["generic types"]) {
                                        case JsonArray gTypeArray: {
                                            genericTypes = gTypeArray.Select(item.Key, (JsonNode? node, string key, int index) => {
                                                switch (node) {
                                                    case JsonObject gTypeJsonObject: {
                                                        string gTypeName;
                                                        switch (gTypeJsonObject["name"]) {
                                                            case JsonValue gTypeJsonValue: {
                                                                string? gTypeString = gTypeJsonValue.ParseValue<string?>(ErrorList, $"[invoke function].[type map].[key ({key})].[generic types (index {index})].[name]", null);
                                                                if (gTypeString is null)
                                                                    return null;

                                                                gTypeName = gTypeString;
                                                                break;
                                                            }
                                                            case null: {
                                                                ErrorList.AddConfigKeyNotFoundError($"[invoke function].[type map].[key ({key})].[generic types (index {index})].[name]");
                                                                return null;
                                                            }
                                                            case not null: {
                                                                ErrorList.AddConfigUnexpectedTypeError($"[invoke function].[type map].[key ({key})].[generic types (index {index})].[name]");
                                                                return null;
                                                            }
                                                        }

                                                        string? gTypeConstraint;
                                                        switch (gTypeJsonObject["constraint"]) {
                                                            case JsonValue gTypeJsonValue: {
                                                                gTypeConstraint = gTypeJsonValue.ParseValue<string?>(ErrorList, $"[invoke function].[type map].[key ({key})].[generic types (index {index})].[constraint]", null);
                                                                break;
                                                            }
                                                            case null: {
                                                                gTypeConstraint = null;
                                                                break;
                                                            }
                                                            case not null: {
                                                                ErrorList.AddConfigUnexpectedTypeError($"[invoke function].[type map].[key ({key})].[generic types (index {index})].[constraint]");
                                                                return null;
                                                            }
                                                        }

                                                        return new GenericType(gTypeName) { Constraint = gTypeConstraint };
                                                    }
                                                    case JsonValue gTypeJsonValue: {
                                                        string? gTypeName = gTypeJsonValue.ParseValue<string?>(ErrorList, $"[invoke function].[type map].[key ({key})].[generic types (index {index})]", null);
                                                        if (gTypeName is null)
                                                            return null;

                                                        return new GenericType(gTypeName);
                                                    }
                                                    default: {
                                                        ErrorList.AddConfigUnexpectedTypeError($"[invoke function].[type map].[key ({key})].[generic types (index {index})]");
                                                        return null;
                                                    }
                                                }
                                            });
                                            break;
                                        }
                                        case JsonObject gTypeJsonObject: {
                                            string gTypeName;
                                            switch (gTypeJsonObject["name"]) {
                                                case JsonValue gTypeJsonValue: {
                                                    string? gTypeString = gTypeJsonValue.ParseValue<string?>(ErrorList, $"[invoke function].[type map].[key ({item.Key})].[generic types].[name]", null);
                                                    if (gTypeString is null)
                                                        continue;

                                                    gTypeName = gTypeString;
                                                    break;
                                                }
                                                case null: {
                                                    ErrorList.AddConfigKeyNotFoundError($"[invoke function].[type map].[key ({item.Key})].[generic types].[name]");
                                                    continue;
                                                }
                                                case not null: {
                                                    ErrorList.AddConfigUnexpectedTypeError($"[invoke function].[type map].[key ({item.Key})].[generic types].[name]");
                                                    continue;
                                                }
                                            }

                                            string? gTypeConstraint;
                                            switch (gTypeJsonObject["constraint"]) {
                                                case JsonValue gTypeJsonValue: {
                                                    gTypeConstraint = gTypeJsonValue.ParseValue<string?>(ErrorList, $"[invoke function].[type map].[key ({item.Key})].[generic types].[constraint]", null);
                                                    break;
                                                }
                                                case null: {
                                                    gTypeConstraint = null;
                                                    break;
                                                }
                                                case not null: {
                                                    ErrorList.AddConfigUnexpectedTypeError($"[invoke function].[type map].[key ({item.Key})].[generic types].[constraint]");
                                                    continue;
                                                }
                                            }

                                            genericTypes = [new GenericType(gTypeName) { Constraint = gTypeConstraint }];
                                            break;
                                        }
                                        case JsonValue gTypeJsonValue: {
                                            string? gTypeName = gTypeJsonValue.ParseValue<string?>(ErrorList, $"[invoke function].[type map].[key ({item.Key})].[generic types]", null);
                                            if (gTypeName is null)
                                                continue;

                                            genericTypes = [new GenericType(gTypeName)];
                                            break;
                                        }
                                        case not null: {
                                            ErrorList.AddConfigUnexpectedTypeError($"[invoke function].[type map].[key ({item.Key})].[generic types]");
                                            goto case null;
                                        }
                                        case null: {
                                            genericTypes = [];
                                            break;
                                        }
                                    }

                                    TypeMap.Add(item.Key, new MappedType(type, genericTypes));
                                    break;
                                }
                                default: {
                                    ErrorList.AddConfigUnexpectedTypeError($"[invoke function].[type map].[key ({item.Key})]");
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError("[invoke function].[type map]");
                        goto case null;
                    }
                    case null: {
                        TypeMap = TypeMapDefault;
                        break;
                    }
                }
                break;
            }
            case not null: {
                ErrorList.AddConfigUnexpectedTypeError("[invoke function]");
                goto case null;
            }
            case null: {
                InvokeFunctionSyncEnabled = INVOKE_FUNCTION_SYNC_ENABLED;
                InvokeFunctionTrySyncEnabled = INVOKE_FUNCTION_TRYSYNC_ENABLED;
                InvokeFunctionAsyncEnabled = INVOKE_FUNCTION_ASYNC_ENABLED;

                InvokeFunctionNamePattern = new FunctionNamePattern(INVOKE_FUNCTION_NAME_PATTERN, INVOKE_FUNCTION_MODULE_TRANSFORM, INVOKE_FUNCTION_FUNCTION_TRANSFORM, INVOKE_FUNCTION_ACTION_TRANSFORM, null!);

                InvokeFunctionActionNameSync = INVOKE_FUNCTION_ACTION_NAME_SYNC;
                InvokeFunctionActionNameTrySync = INVOKE_FUNCTION_ACTION_NAME_TRYSYNC;
                InvokeFunctionActionNameAsync = INVOKE_FUNCTION_ACTION_NAME_ASYNC;
                
                PromiseOnlyAsync = PROMISE_ONLY_ASYNC;
                PromiseAppendAsync = PROMISE_APPEND_ASYNC;

                TypeMap = TypeMapDefault;
                break;
            }
        }


        // PreloadNamePattern, PreloadAllModulesName
        switch (root["preload function"]) {
            case JsonObject jsonObject: {
                switch (jsonObject["name pattern"]) {
                    case JsonObject namePatternJsonObject: {
                        string namePattern = namePatternJsonObject.ParseValue("pattern", ErrorList, "[preload function].[name pattern].[pattern]", PRELOAD_NAME_PATTERN);
                        NameTransform moduleTransform = namePatternJsonObject.ParseNameTransform("module transform", ErrorList, "[preload function].[name pattern].[module transform]", PRELOAD_MODULE_TRANSFORM);
                        PreloadNamePattern = new ModuleNamePattern(namePattern, moduleTransform, ErrorList);
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError("[preload function].[name pattern]");
                        goto case null;
                    }
                    case null: {
                        PreloadNamePattern = new ModuleNamePattern(PRELOAD_NAME_PATTERN, PRELOAD_MODULE_TRANSFORM, null!);
                        break;
                    }
                }

                PreloadAllModulesName = jsonObject.ParseValue("all modules name", ErrorList, "[preload function].[all modules name]", PRELOAD_ALL_MODULES_NAME);
                break;
            }
            case not null: {
                ErrorList.AddConfigUnexpectedTypeError("[preload function]");
                goto case null;
            }
            case null: {
                PreloadNamePattern = new ModuleNamePattern(PRELOAD_NAME_PATTERN, PRELOAD_MODULE_TRANSFORM, null!);
                PreloadAllModulesName = PRELOAD_ALL_MODULES_NAME;
                break;
            }
        }


        // ModuleGrouping, ModuleGroupingNamePattern
        switch (root["module grouping"]) {
            case JsonValue valueNode: {
                ModuleGrouping = valueNode.ParseValue(ErrorList, "[module grouping]", MODULE_GROUPING);
                ModuleGroupingNamePattern = new ModuleNamePattern(MODULE_GROUPING_NAME_PATTERN, MODULE_GROUPING_MODULE_TRANSFORM, null!);
                break;
            }
            case JsonObject jsonObject: {
                ModuleGrouping = jsonObject.ParseValue("enabled", ErrorList, "[module grouping].[enabled]", MODULE_GROUPING);

                switch (jsonObject["interface name pattern"]) {
                    case JsonObject namePatternJsonObject: {
                        string namePattern = namePatternJsonObject.ParseValue("pattern", ErrorList, "[module grouping].[interface name pattern].pattern", MODULE_GROUPING_NAME_PATTERN);
                        NameTransform moduleTransform = namePatternJsonObject.ParseNameTransform("module transform", ErrorList, "[module grouping].[interface name pattern].[module transform]", MODULE_GROUPING_MODULE_TRANSFORM);
                        ModuleGroupingNamePattern = new ModuleNamePattern(namePattern, moduleTransform, ErrorList);
                        break;
                    }
                    case not null: {
                        ErrorList.AddConfigUnexpectedTypeError("[module grouping].[interface name pattern]");
                        goto case null;
                    }
                    case null: {
                        ModuleGroupingNamePattern = new ModuleNamePattern(MODULE_GROUPING_NAME_PATTERN, MODULE_GROUPING_MODULE_TRANSFORM, null!);
                        break;
                    }
                }
                break;
            }
            case not null: {
                ErrorList.AddConfigUnexpectedTypeError("[module grouping]");
                goto case null;
            }
            case null: {
                ModuleGrouping = MODULE_GROUPING;
                ModuleGroupingNamePattern = new ModuleNamePattern(MODULE_GROUPING_NAME_PATTERN, MODULE_GROUPING_MODULE_TRANSFORM, null!);
                break;
            }
        }


        // JSRuntimeSyncEnabled, JSRuntimeTrySyncEnabled, JSRuntimeAsyncEnabled
        switch (root["js runtime"]) {
            case JsonObject jsonObject: {
                JSRuntimeSyncEnabled = jsonObject.ParseValue("sync enabled", ErrorList, "[js runtime].[sync enabled]", JSRUNTIME_SYNC_ENABLED);
                JSRuntimeTrySyncEnabled = jsonObject.ParseValue("trysync enabled", ErrorList, "[js runtime].[trysync enabled]", JSRUNTIME_TRYSYNC_ENABLED);
                JSRuntimeAsyncEnabled = jsonObject.ParseValue("async enabled", ErrorList, "[js runtime].[async enabled]", JSRUNTIME_ASYNC_ENABLED);
                break;
            }
            case not null: {
                ErrorList.AddConfigUnexpectedTypeError("[js runtime]");
                goto case null;
            }
            case null: {
                JSRuntimeSyncEnabled = JSRUNTIME_SYNC_ENABLED;
                JSRuntimeTrySyncEnabled = JSRUNTIME_TRYSYNC_ENABLED;
                JSRuntimeAsyncEnabled = JSRUNTIME_ASYNC_ENABLED;
                break;
            }
        }


        ServiceExtension = root.ParseValue("service extension", ErrorList, "[service extension]", SERVICE_EXTENSION);
    }
    
    /// <summary>
    /// Converts this instance as a json-file.
    /// </summary>
    public string ToJson() {
        StringBuilder builder = new(100);


        string inputPath;
        if (InputPath.Length == 0)
            inputPath = string.Empty;
        else {
            builder.Clear();

            foreach ((string include, string[] excludes, bool moduleFiles, string? fileModulePath) in InputPath) {
                string excludesJson = excludes.Length switch {
                    0 => "[]",
                    1 => $"""[ "{excludes[0]}" ]""",
                    _ => $"""
                        [
                                "{string.Join("\",\n        \"", excludes)}"
                              ]
                        """
                };
                builder.Append($$"""
                    
                        {
                          "include": "{{include}}",
                          "excludes": {{excludesJson}},
                          "module files": {{(moduleFiles ? "true" : "false")}}
                    """);
                if (fileModulePath is not null)
                    builder.Append($"""
                        ,
                              "module path": "{fileModulePath}"
                        """);
                builder.Append("\n    },");
            }

            builder.Length--;
            builder.Append("\n  ");
            inputPath = builder.ToString();
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

            foreach (KeyValuePair<string, MappedType> pair in TypeMap) {
                builder.Append("\n      \"");
                builder.Append(pair.Key);
                builder.Append(@""": ");
                if (pair.Value.GenericTypes.Length == 0) {
                    builder.Append('"');
                    builder.Append(pair.Value.Type);
                    builder.Append('"');
                }
                else {
                    builder.Append($$"""
                        {
                                "type": "{{pair.Value.Type}}",
                                "generic types": [

                        """);
                    foreach (GenericType genericType in pair.Value.GenericTypes) {
                        builder.Append($$"""
                                      {
                                        "name": "{{genericType.Name}}",
                                        "constraint": {{(genericType.Constraint is not null ? $"\"{genericType.Constraint}\"" : "null")}}
                                      },

                            """);

                    }
                    builder.Length -= 2;
                    builder.Append("""

                                ]
                              }
                        """);
                }
                builder.Append(',');
            }
            builder.Length--;
            builder.Append("\n    ");
            typeMap = builder.ToString();
        }

        return $$"""
            {
              "webroot path": "{{WebRootPath}}",
              "input path": [{{inputPath}}],
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
                  "append async": {{(PromiseAppendAsync ? "true" : "false")}}
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
              "module grouping": {
                "enabled": {{(ModuleGrouping ? "true" : "false")}},
                "interface name pattern": {
                  "pattern": "{{ModuleGroupingNamePattern.NamePattern}}",
                  "module transform": "{{ModuleGroupingNamePattern.ModuleTransform}}"
                }
              },
              "js runtime": {
                "sync enabled": {{(JSRuntimeSyncEnabled ? "true" : "false")}},
                "trysync enabled": {{(JSRuntimeTrySyncEnabled ? "true" : "false")}},
                "async enabled": {{(JSRuntimeAsyncEnabled ? "true" : "false")}}
              },
              "service extension": {{(ServiceExtension ? "true" : "false")}}
            }

            """;
    }


    /// <summary>
    /// replaces '\' with '/' and removes trailing slash
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string Normalize(string path) {
        path = path.Replace('\\', '/');

        if (path is [.., '/'])
            path = path[..^1];

        return path;
    }


    #region IEquatable

    public static bool operator ==(Config left, Config right) => left.Equals(right);

    public static bool operator !=(Config left, Config right) => !left.Equals(right);

    public override bool Equals(object obj)
        => obj switch {
            Config other => Equals(other),
            _ => false
        };

    public bool Equals(Config other) {
        if (WebRootPath != other.WebRootPath)
            return false;
        if (!InputPath.SequenceEqual(other.InputPath))
            return false;
        if (!UsingStatements.SequenceEqual(other.UsingStatements))
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

        if (!TypeMap.SequenceEqual(other.TypeMap))
            return false;

        if (PreloadNamePattern != other.PreloadNamePattern)
            return false;
        if (PreloadAllModulesName != other.PreloadAllModulesName)
            return false;

        if (ModuleGrouping != other.ModuleGrouping)
            return false;
        if (ModuleGroupingNamePattern != other.ModuleGroupingNamePattern)
            return false;

        if (JSRuntimeSyncEnabled != other.JSRuntimeSyncEnabled)
            return false;
        if (JSRuntimeTrySyncEnabled != other.JSRuntimeTrySyncEnabled)
            return false;
        if (JSRuntimeAsyncEnabled != other.JSRuntimeAsyncEnabled)
            return false;

        if (ServiceExtension != other.ServiceExtension)
            return false;

        if (!ErrorList.SequenceEqual(other.ErrorList))
            return false;

        return true;
    }

    public override int GetHashCode() {
        int hashCode = WebRootPath.GetHashCode();
        hashCode = CombineList(hashCode, InputPath);
        hashCode = CombineList(hashCode, UsingStatements);

        hashCode = Combine(hashCode, InvokeFunctionSyncEnabled.GetHashCode());
        hashCode = Combine(hashCode, InvokeFunctionTrySyncEnabled.GetHashCode());
        hashCode = Combine(hashCode, InvokeFunctionAsyncEnabled.GetHashCode());

        hashCode = Combine(hashCode, InvokeFunctionNamePattern.GetHashCode());

        hashCode = Combine(hashCode, InvokeFunctionActionNameSync.GetHashCode());
        hashCode = Combine(hashCode, InvokeFunctionActionNameTrySync.GetHashCode());
        hashCode = Combine(hashCode, InvokeFunctionActionNameAsync.GetHashCode());

        hashCode = Combine(hashCode, PromiseOnlyAsync.GetHashCode());
        hashCode = Combine(hashCode, PromiseAppendAsync.GetHashCode());

        hashCode = CombineList(hashCode, TypeMap);
        
        hashCode = Combine(hashCode, PreloadNamePattern.GetHashCode());
        hashCode = Combine(hashCode, PreloadAllModulesName.GetHashCode());

        hashCode = Combine(hashCode, ModuleGrouping.GetHashCode());
        hashCode = Combine(hashCode, ModuleGroupingNamePattern.GetHashCode());

        hashCode = Combine(hashCode, JSRuntimeSyncEnabled.GetHashCode());
        hashCode = Combine(hashCode, JSRuntimeTrySyncEnabled.GetHashCode());
        hashCode = Combine(hashCode, JSRuntimeAsyncEnabled.GetHashCode());

        hashCode = Combine(hashCode, ServiceExtension.GetHashCode());

        hashCode = CombineList(hashCode, ErrorList);

        return hashCode;


        static int CombineList<T>(int hashCode, IEnumerable<T> list) where T : notnull {
            foreach (T item in list)
                hashCode = Combine(hashCode, item.GetHashCode());
            return hashCode;
        }

        static int Combine(int h1, int h2) {
            uint r = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)r + h1 ^ h2;
        }
    }

    #endregion
}


file static class JsonNodeExtension {
    internal static T ParseValue<T>(this JsonNode node, string key, List<Diagnostic> errorList, string errorKey, T defaultValue)
        => node[key].ParseValue(errorList, errorKey, defaultValue);

    internal static T ParseValue<T>(this JsonNode? node, List<Diagnostic> errorList, string errorKey, T defaultValue) {
        if (node is not JsonNode jsonNode)
            return defaultValue;

        if (jsonNode is not JsonValue jsonValue) {
            errorList.AddConfigUnexpectedTypeError(errorKey);
            return defaultValue;
        }

        if (!jsonValue.TryGetValue(out T? value)) {
            if (value is string)
                errorList.AddConfigStringExpectedError(errorKey);
            else if (value is bool)
                errorList.AddConfigBoolExpectedError(errorKey);
            else
                errorList.AddConfigUnexpectedTypeError(errorKey);
            return defaultValue;
        }

        return value;
    }


    internal static NameTransform ParseNameTransform(this JsonNode node, string key, List<Diagnostic> errorList, string errorKey, NameTransform defaultValue)
        => node[key].ParseNameTransform(errorList, errorKey, defaultValue);

    internal static NameTransform ParseNameTransform(this JsonNode? node, List<Diagnostic> errorList, string errorKey, NameTransform defaultValue) {
        if (node is not JsonNode jsonNode)
            return defaultValue;

        if (jsonNode is not JsonValue jsonValue) {
            errorList.AddConfigUnexpectedTypeError(errorKey);
            return defaultValue;
        }

        if (!jsonValue.TryGetValue(out string? value)) {
            errorList.AddConfigNameTransformExpectedError(errorKey);
            return defaultValue;
        }

        if (!Enum.TryParse(value.Replace(" ", ""), ignoreCase: true, out NameTransform nameTransform)) {
            errorList.AddConfigNameTransformExpectedError(errorKey);
            return defaultValue;
        }

        return nameTransform;
    }


    internal static string[] Select(this JsonArray array, Func<JsonNode?, int, string?> parseFunction) {
        string[] result = new string[array.Count];

        int numberOfWrongEntries = 0;
        for (int i = 0; i < array.Count; i++) {
            string? value = parseFunction(array[i], i);
            if (value is not null)
                result[i] = value;
            else
                numberOfWrongEntries++;
        }

        if (numberOfWrongEntries > 0) {
            string[] correctedresult = new string[array.Count - numberOfWrongEntries];

            int index = 0;
            for (int i = 0; i < array.Count; i++)
                if (result[i] is not null)
                    correctedresult[index++] = result[i];

            return correctedresult;
        }

        return result;
    }

    internal static InputPath[] Select(this JsonArray array, Func<JsonNode?, int, InputPath?> parseFunction) {
        InputPath[] result = new InputPath[array.Count];

        int numberOfWrongEntries = 0;
        for (int i = 0; i < array.Count; i++) {
            InputPath? value = parseFunction(array[i], i);
            if (value is not null)
                result[i] = value.Value;
            else
                numberOfWrongEntries++;
        }

        if (numberOfWrongEntries > 0) {
            InputPath[] correctedresult = new InputPath[array.Count - numberOfWrongEntries];

            int index = 0;
            for (int i = 0; i < array.Count; i++)
                if (result[i] != default)
                    correctedresult[index++] = result[i];

            return correctedresult;
        }

        return result;
    }

    internal static GenericType[] Select(this JsonArray array, string key, Func<JsonNode?, string, int, GenericType?> parseFunction) {
        GenericType[] result = new GenericType[array.Count];

        int numberOfWrongEntries = 0;
        for (int i = 0; i < array.Count; i++) {
            GenericType? value = parseFunction(array[i], key, i);
            if (value is not null)
                result[i] = value.Value;
            else
                numberOfWrongEntries++;
        }

        if (numberOfWrongEntries > 0) {
            GenericType[] correctedresult = new GenericType[array.Count - numberOfWrongEntries];

            int index = 0;
            for (int i = 0; i < array.Count; i++)
                if (result[i] != default)
                    correctedresult[index++] = result[i];

            return correctedresult;
        }

        return result;
    }
}
