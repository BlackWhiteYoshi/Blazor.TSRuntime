﻿using Microsoft.CodeAnalysis;

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


    public static void AddConfigFunctionTransformMissingActionError(this List<Diagnostic> errorList, string jsonKey)
        => errorList.Add(Diagnostic.Create(ConfigFunctionTransformMissingAction, null, [jsonKey]));

    private static DiagnosticDescriptor ConfigFunctionTransformMissingAction { get; } = new(
        id: "BTS012",
        title: "config function transform missing action",
        messageFormat: "malformed config: '{0}' should contain '#action#' when 2 or more method types are enabled, otherwise it leads to duplicate method naming",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public static void AddInputPathNoStartingSlashError(this List<Diagnostic> errorList, string jsonKey)
        => errorList.Add(Diagnostic.Create(InputPathNoStartingSlash, null, [jsonKey]));

    private static DiagnosticDescriptor InputPathNoStartingSlash { get; } = new(
        id: "BTS013",
        title: "config 'input path' has no starting slash",
        messageFormat: "malformed config: '{0}' should start with '/'",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public static void AddModulePathNoJsExtensionError(this List<Diagnostic> errorList, string jsonKey)
        => errorList.Add(Diagnostic.Create(ModulePathNoJsExtension, null, [jsonKey]));

    private static DiagnosticDescriptor ModulePathNoJsExtension { get; } = new(
        id: "BTS014",
        title: "config 'module path' has no '.js' extension",
        messageFormat: "malformed config: '{0}' should end with '.js'",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    #endregion


    #region Parsing

    public static void AddFunctionParseError(this List<Diagnostic> errorList, DiagnosticDescriptor descriptor, string filePath, int lineNumber, int position)
        => errorList.Add(Diagnostic.Create(descriptor, null, [filePath, lineNumber, position]));

    public static DiagnosticDescriptor FileMissingOpenBracket { get; } = new(
        id: "BTS015",
        title: "invalid file: missing '('",
        messageFormat: "invalid file: '{0}' at line {1}: missing '(' after column {2} (the token that indicates the start of function parameters)",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor FileMissingClosingGenericBracket { get; } = new(
        id: "BTS016",
        title: "invalid file: missing '('",
        messageFormat: "invalid file: '{0}' at line {1}: missing '>' after column {2} (the token that marks the end of generics)",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor FileNoParameterEnd { get; } = new(
        id: "BTS017",
        title: "invalid file: no end of parameter",
        messageFormat: "invalid file: '{0}' at line {1}: missing ')' after column {2} (the token that marks end of parameters)",
        category: "Blazor.TSRuntime",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    #endregion
}
