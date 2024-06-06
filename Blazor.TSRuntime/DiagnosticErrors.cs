using Microsoft.CodeAnalysis;

namespace TSRuntime;

public static class DiagnosticErrors {
    public static Diagnostic CreateNoConfigFileError()
        => Diagnostic.Create(NoConfigFile, null);

    private static DiagnosticDescriptor NoConfigFile { get; } = new(
        id: "BTS001",
        title: "missing config file",
        messageFormat: "No tsruntime.json file found. Make sure the file ends with 'tsruntime.json' and is added with the <AdditionalFiles Include=\"...\" /> directive.",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static Diagnostic CreateMultipleConfigFilesError()
        => Diagnostic.Create(MultipleConfigFiles, null);

    private static DiagnosticDescriptor MultipleConfigFiles { get; } = new(
        id: "BTS002",
        title: "multiple config files",
        messageFormat: "multiple tsruntime.json files found. Make sure only 1 file ends with 'tsruntime.json'",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static Diagnostic CreateFileReadingError(string textFilePath)
        => Diagnostic.Create(FileReadingError, null, [textFilePath]);

    private static DiagnosticDescriptor FileReadingError { get; } = new(
        id: "BTS003",
        title: "file reading error",
        messageFormat: "File reading error: '{0}' could not be accessed for reading.",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);



    #region Config

    public static void AddConfigInvalidError(this List<Diagnostic> errorList)
        => errorList.Add(Diagnostic.Create(ConfigInvalid, null));

    private static DiagnosticDescriptor ConfigInvalid { get; } = new(
        id: "BTS004",
        title: "config is invalid json",
        messageFormat: "config is in an invalid json format",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    public static void AddConfigKeyNotFoundError(this List<Diagnostic> errorList, string jsonKey)
        => errorList.Add(Diagnostic.Create(ConfigKeyNotFound, null, [jsonKey]));

    private static DiagnosticDescriptor ConfigKeyNotFound { get; } = new(
        id: "BTS005",
        title: "config key not found",
        messageFormat: "invalid config: key '{0}' not found, default is taken instead",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public static void AddConfigUnexpectedTypeError(this List<Diagnostic> errorList, string jsonKey)
        => errorList.Add(Diagnostic.Create(ConfigUnexpectedType, null, [jsonKey]));

    private static DiagnosticDescriptor ConfigUnexpectedType { get; } = new(
        id: "BTS006",
        title: "config unexpected type",
        messageFormat: "invalid config: '{0}' has unexpected type, default is taken instead",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public static void AddConfigStringExpectedError(this List<Diagnostic> errorList, string jsonKey)
        => errorList.Add(Diagnostic.Create(ConfigStringExpected, null, [jsonKey]));

    private static DiagnosticDescriptor ConfigStringExpected { get; } = new(
        id: "BTS007",
        title: "config string expected",
        messageFormat: "invalid config: '{0}' has wrong type, must be a string, default is taken instead. If you want to have null, use string literal \"null\" instead.",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public static void AddConfigBoolExpectedError(this List<Diagnostic> errorList, string jsonKey)
        => errorList.Add(Diagnostic.Create(ConfigBoolExpected, null, [jsonKey]));

    private static DiagnosticDescriptor ConfigBoolExpected { get; } = new(
        id: "BTS008",
        title: "config bool expected",
        messageFormat: "invalid config: '{0}' has wrong type, must be either \"true\" or \"false\", default is taken instead",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public static void AddConfigNamePatternMissingEndTagError(this List<Diagnostic> errorList)
        => errorList.Add(Diagnostic.Create(ConfigNamePatternMissingEndTag, null));

    private static DiagnosticDescriptor ConfigNamePatternMissingEndTag { get; } = new(
        id: "BTS009",
        title: "config name pattern missing '#'",
        messageFormat: "invalid config: name pattern has starting '#' but missing closing '#'",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public static void AddConfigNamePatternInvalidVariableError(this List<Diagnostic> errorList, string invalidVariable, string[] validVaraibleNames)
        => errorList.Add(Diagnostic.Create(ConfigNamePatternInvalidVariable, null, [invalidVariable, string.Join("\", \"", validVaraibleNames)]));

    private static DiagnosticDescriptor ConfigNamePatternInvalidVariable { get; } = new(
        id: "BTS010",
        title: "config nametransform expected",
        messageFormat: "invalid config: name pattern has invalid variable \"{0}\". Allowed values are: \"{1}\"",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public static void AddConfigNameTransformExpectedError(this List<Diagnostic> errorList, string jsonKey)
        => errorList.Add(Diagnostic.Create(ConfigNameTransformExpected, null, [jsonKey]));

    private static DiagnosticDescriptor ConfigNameTransformExpected { get; } = new(
        id: "BTS011",
        title: "config nametransform expected",
        messageFormat: "invalid config: '{0}' has wrong value, must be either \"first upper case\", \"first lower case\", \"upper case\", \"lower case\" or \"none\"",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    #endregion



    #region Parsing

    public static void AddModulePathNoJsExtensionError(this List<Diagnostic> errorList, string filePath, string modulePath)
        => errorList.Add(Diagnostic.Create(ModulePathNoJsExtension, null, [filePath, modulePath]));

    private static DiagnosticDescriptor ModulePathNoJsExtension { get; } = new(
        id: "BTS012",
        title: "fileModulePath has no '.js' extension",
        messageFormat: "bad input path: {{ include: '{0}', fileModulePath: '{1}' }}. fileModulePath should end with '.js'",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    #endregion
}
