using Microsoft.CodeAnalysis;
using TSRuntime.Parsing;

namespace TSRuntime.Tests;

public static class ParserTests {
    #region TSParamter

    [Theory]
    [InlineData("number", "number", false, false, false, false)]
    [InlineData("string", "string", false, false, false, false)]
    [InlineData("asdf", "asdf", false, false, false, false)]

    [InlineData("number | null", "number", true, false, false, false)]
    [InlineData("number | undefined", "number", false, false, false, true)]
    [InlineData("number | null | undefined", "number", true, false, false, true)]
    [InlineData("number | undefined | null", "number", true, false, false, true)]
    [InlineData("null | undefined | number", "number", true, false, false, true)]
    [InlineData("undefined | null | number", "number", true, false, false, true)]
    [InlineData("null | number | undefined", "number", true, false, false, true)]
    [InlineData("undefined | number | null", "number", true, false, false, true)]


    [InlineData("number[]", "number", false, true, false, false)]
    [InlineData("readonly number[]", "number", false, true, false, false)]
    [InlineData("Array<number>", "number", false, true, false, false)]
    [InlineData("readonly Array<number>", "number", false, true, false, false)]
    [InlineData("number[] | null", "number", false, true, true, false)]
    [InlineData("number[] | undefined", "number", false, true, false, true)]
    [InlineData("number[] | null | undefined", "number", false, true, true, true)]
    [InlineData("Array<number> | null", "number", false, true, true, false)]
    [InlineData("Array<number> | undefined", "number", false, true, false, true)]
    [InlineData("Array<number> | null | undefined", "number", false, true, true, true)]

    [InlineData("(number | null)[]", "number", true, true, false, false)]
    [InlineData("(number | undefined)[]", "number", true, true, false, false)]
    [InlineData("(number | null | undefined)[]", "number", true, true, false, false)]
    [InlineData("(number | null)[] | null", "number", true, true, true, false)]
    [InlineData("(number | undefined)[] | undefined", "number", true, true, false, true)]
    [InlineData("(number | null | undefined)[] | null | undefined", "number", true, true, true, true)]
    [InlineData("readonly (number | null)[] | null", "number", true, true, true, false)]
    [InlineData("readonly (number | undefined)[] | undefined", "number", true, true, false, true)]
    [InlineData("readonly (number | null | undefined)[] | null | undefined", "number", true, true, true, true)]

    [InlineData("[number, string]", "[number, string]", false, false, false, false)]
    [InlineData("readonly [number, string]", "[number, string]", false, false, false, false)]
    public static void ParsingParameter_Works(string input, string type, bool typeNullable, bool array, bool arrayNullable, bool optional) {
        TSParameter parameter = new();

        parameter.ParseType(input);

        Assert.Equal(type, parameter.type);
        Assert.Equal(typeNullable, parameter.typeNullable);
        Assert.Equal(array, parameter.array);
        Assert.Equal(arrayNullable, parameter.arrayNullable);
        Assert.Equal(optional, parameter.optional);
    }

    [Theory]
    [InlineData("", "", false)]
    [InlineData("a", "a", false)]
    [InlineData("asdf", "asdf", false)]
    [InlineData("a?", "a", true)]
    [InlineData("asdf?", "asdf", true)]
    public static void ParsingParameterName_Works(string input, string name, bool optional) {
        TSParameter parameter = new();

        parameter.ParseName(input);

        Assert.Equal(name, parameter.name);
        Assert.Equal(optional, parameter.optional);
    }

    #endregion


    #region TSFunction

    [Theory]
    [InlineData("export declare function getCookies(): string;", "getCookies", "string", false)]
    [InlineData("export function asdf(): qwer;", "asdf", "qwer", false)]
    [InlineData("export function Test(a?: number): void;", "Test", "void", false)]
    [InlineData("export function longRunningTask(): Promise<void>;", "longRunningTask", "void", true)]
    [InlineData("export function longRunningTask2(): Promise<something>;", "longRunningTask2", "something", true)]
    public static void ParsingFunction(string input, string name, string returnType, bool promise) {
        TSFunction function = TSFunction.ParseTSFunction(input)!;

        Assert.Equal(name, function.Name);
        Assert.Equal(returnType, function.ReturnType.type);
        Assert.Equal(promise, function.ReturnPromise);
    }

    [Theory]
    [InlineData("export function generic1<T>(): void;", "generic1", (string[])["T"])]
    [InlineData("export function generic3<Type, Key, Value>(): void;", "generic3", (string[])["Type", "Key", "Value"])]
    [InlineData("export function genericConstraint<Type extends HTMLElement>(): void;", "genericConstraint", (string[])["Type"])]
    [InlineData("export function genericConstraintGeneric<Type extends Map<K, V>>(): void;", "genericConstraintGeneric", (string[])["Type"])]
    [InlineData("export function genericKeyofConstraint<Type, Key extends keyof Type>(): void;", "genericKeyofConstraint", (string[])["Type", "Key"])]
    public static void ParsingGenericFunction(string input, string name, string[] generics) {
        TSFunction function = TSFunction.ParseTSFunction(input)!;

        Assert.Equal(name, function.Name);
        Assert.Equal(generics.Length, function.Generics.Length);
        for (int i = 0; i < function.Generics.Length; i++)
            Assert.Equal(generics[i], function.Generics[i]);
    }

    [Theory]
    [InlineData("\n\n", "")]
    [InlineData("/**/\n", "")]
    [InlineData("/***/\n", "")]
    [InlineData("/**example*/\n", "example")]
    [InlineData("/**   example  */\n", "example")]
    [InlineData("/**  * example  */\n", "example")]
    [InlineData("/**  ** example  */\n", "* example")]
    [InlineData("/**\n*\n* example  */\n", "example")]
    [InlineData("/** example\n */\n", "example")]
    [InlineData("/**\n * a legit example\n * some more text\n *\n * some note\n */\n", "a legit example<br/>some more text<br/><br/>some note")]
    public static void ParsingSummary(string input, string summary) {
        const string FUNCTION_DECLARATION = "export function test(): void;";
        TSFunction function = TSFunction.ParseTSFunction(FUNCTION_DECLARATION)!;
        function.ParseTSSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        Assert.Equal(summary, function.Summary);
    }

    [Theory]
    [InlineData("/**@remarks*/\n", "", "")]
    [InlineData("/**@remarks example*/\n", "", "example")]
    [InlineData("/**summaryExample @remarks example*/\n", "summaryExample", "example")]
    [InlineData("/**summaryExample\n * @remarks example*/\n", "summaryExample", "example")]
    [InlineData("/**summaryExample\n * @unkownT example*/\n", "summaryExample", "")]
    [InlineData("/**summaryExample\n * @unkownT example@remarks ex*/\n", "summaryExample", "ex")]
    [InlineData("/**summaryExample\n * @unkownT @remarks ex*/\n", "summaryExample", "ex")]
    [InlineData("/**\n * a legit example\n * some more text\n *\n * @remarks example\n */\n", "a legit example<br/>some more text", "example")]
    public static void ParsingSummaryWithTag(string input, string summary, string remarks) {
        const string FUNCTION_DECLARATION = "export function test(): void;";
        TSFunction function = TSFunction.ParseTSFunction(FUNCTION_DECLARATION)!;
        function.ParseTSSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        Assert.Equal(summary, function.Summary);
        Assert.Equal(remarks, function.Remarks);
    }

    [Theory]
    [InlineData("/**@param*/\n", "", "")]
    [InlineData("/**@param integer - example*/\n", "", "example")]
    [InlineData("/**summaryExample @param integer example*/\n", "summaryExample", "example")]
    [InlineData("/**summaryExample\n * @param integer - example*/\n", "summaryExample", "example")]
    [InlineData("/**summaryExample\n * @param */\n", "summaryExample", "")]
    [InlineData("/**summaryExample\n * @param example*/\n", "summaryExample", "")]
    [InlineData("/**summaryExample\n * @param example @param integer ex*/\n", "summaryExample", "ex")]
    [InlineData("/**\n * a legit example\n * some more text\n *\n * @param integer - example\n */\n", "a legit example<br/>some more text", "example")]
    public static void ParsingSummaryWithParamTag(string input, string summary, string parameter) {
        const string FUNCTION_DECLARATION = "export function test(integer: number): void;";
        TSFunction function = TSFunction.ParseTSFunction(FUNCTION_DECLARATION)!;
        function.ParseTSSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        Assert.Equal(summary, function.Summary);
        Assert.Equal(parameter, function.ParameterList[0].summary);
    }


    [Fact]
    public static void ParsingFunction_WrongStartRetunrsNull() {
        TSFunction? result = TSFunction.ParseTSFunction("asdf");
        Assert.Null(result);
    }

    [Fact]
    public static void ParsingFunction_MissingOpenBracketError() {
        TSFunction? result = TSFunction.ParseTSFunction("export declare function Example[);");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "E_Module.d.ts", 1, result.Error.position);
        Assert.Equal("invalid d.ts file: 'E_Module.d.ts' at line 1: missing '(' after column 24 (the token that indicates the start of function parameters)", errorList[0].GetMessage());
    }

    [Fact]
    public static void ParsingFunction_MissingClosingGenericBracketError() {
        TSFunction? result = TSFunction.ParseTSFunction("export function Example<T]();");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "E_Module.d.ts", 1, result.Error.position);
        Assert.Equal("invalid d.ts file: 'E_Module.d.ts' at line 1: missing '>' after column 23 (the token that marks the end of generics)", errorList[0].GetMessage());
    }

    [Fact]
    public static void ParsingFunction_MissingColonError() {
        TSFunction? result = TSFunction.ParseTSFunction("export declare function Example(number myNumber);");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "E_Module.d.ts", 1, result.Error.position);
        Assert.Equal("invalid d.ts file: 'E_Module.d.ts' at line 1: missing ':' after column 32 (the token that seperates name and type)", errorList[0].GetMessage());
    }

    [Fact]
    public static void ParsingFunction_NoParameterEndError() {
        TSFunction? result = TSFunction.ParseTSFunction("export declare function Example(myNumber: number];");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "E_Module.d.ts", 1, result.Error.position);
        Assert.Equal("invalid d.ts file: 'E_Module.d.ts' at line 1: missing ')' after column 42 (the token that marks end of parameters)", errorList[0].GetMessage());
    }

    [Fact]
    public static void ParsingFunction_MissingEndingSemicolonError() {
        TSFunction? result = TSFunction.ParseTSFunction("export declare function Example()");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "E_Module.d.ts", 1, result.Error.position);
        Assert.Equal("invalid d.ts file: 'E_Module.d.ts' at line 1: missing ';' at at column 32", errorList[0].GetMessage());
    }

    #endregion


    #region TSModule

    private const string RootFolder = "/CorePaserTestData";
    private const string MODULE = "TestModule";
    private const string MODULE_CONTENT = "export declare function Test(a: number, b: string): number;\n";

    [Fact]
    public static void ParsingModule_MetadataOnlyHasEmptyFunctionList() {
        TSModule module = new($"{RootFolder}/{MODULE}.d.ts", null, []);

        Assert.NotEqual(string.Empty, module.FilePath);
        Assert.NotEqual(string.Empty, module.URLPath);
        Assert.NotEqual(string.Empty, module.Name);
        Assert.Empty(module.FunctionList);
    }

    [Fact]
    public static void ParsingModule_FunctionsOnlyHasEmptyMetaData() {
        TSModule module = new($"{RootFolder}/{MODULE}.d.ts", null, []);
        module = module.ParseFunctions(MODULE_CONTENT, null!);

        Assert.Equal($"{RootFolder}/{MODULE}.d.ts", module.FilePath);
        Assert.Equal($"{RootFolder}/TestModule.js", module.URLPath);
        Assert.Equal("TestModule", module.Name);
        Assert.NotEmpty(module.FunctionList);
    }

    [Fact]
    public static void ParsingModuleWithModulePath() {
        const string modulePath = "somePath";
        TSModule module = new($"{RootFolder}/{MODULE}.d.ts", $"{modulePath}.js", []);

        Assert.Equal($"{RootFolder}/{MODULE}.d.ts", module.FilePath);
        Assert.Equal($"/{modulePath}.js", module.URLPath);
        Assert.Equal(modulePath, module.Name);
    }


    [Theory]
    [InlineData("test")]
    [InlineData("/test")]
    [InlineData("test.js")]
    [InlineData("/test.js")]
    public static void ParseMetaDataModulePath(string input) {
        TSModule tSModule = new(input, null, []);

        Assert.Equal("/test.js", tSModule.URLPath);
        Assert.Equal("test", tSModule.Name);
    }

    #endregion
}
