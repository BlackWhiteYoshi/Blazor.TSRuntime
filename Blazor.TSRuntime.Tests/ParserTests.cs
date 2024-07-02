using Microsoft.CodeAnalysis;
using TSRuntime.Parsing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TSRuntime.Tests;

public static class ParserTests {
    #region TSParamter

    [Theory]
    [InlineData("", "", false)]
    [InlineData("a", "a", false)]
    [InlineData("asdf", "asdf", false)]
    [InlineData("a?", "a", true)]
    [InlineData("asdf?", "asdf", true)]
    public static void ParsingParameterName(string input, string name, bool optional) {
        TSParameter parameter = new();
        parameter.ParseName(input);

        Assert.Equal(name, parameter.name);
        Assert.Equal(optional, parameter.optional);
    }

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

    [InlineData("number  |    null|undefined", "number", true, false, false, true)]
    [InlineData("readonly     number    [   ]", "number", false, true, false, false)]
    [InlineData("readonly   Array<   number  >", "number", false, true, false, false)]
    [InlineData("(   number    |null|     undefined     )   [ ]|null|undefined", "number", true, true, true, true)]
    public static void ParsingParameterType(string input, string type, bool typeNullable, bool array, bool arrayNullable, bool optional) {
        TSParameter parameter = new();
        parameter.ParseType(input);

        Assert.Equal(type, parameter.type);
        Assert.Equal(typeNullable, parameter.typeNullable);
        Assert.Equal(array, parameter.array);
        Assert.Equal(arrayNullable, parameter.arrayNullable);
        Assert.Equal(optional, parameter.optional);
    }

    [Theory]
    [InlineData("(str: string) => number", (string[])["string", "number"], false)]
    [InlineData("() => void", (string[])["void"], false)]
    [InlineData("(str: string) => Promise<number>", (string[])["string", "number"], true)]
    [InlineData("(a: boolean, b: number) => void", (string[])["boolean", "number", "void"], false)]
    [InlineData("(a:boolean,b:number)=>void", (string[])["boolean", "number", "void"], false)]
    [InlineData("(   a :      boolean ,  b  :   number    ) =>    void", (string[])["boolean", "number", "void"], false)]
    public static void ParsingParameterTypeCallback(string input, string[] callbackTypes, bool typeCallbackPromise) {
        TSParameter parameter = new();
        parameter.ParseType(input);


        Assert.Null(parameter.type);
        Assert.Equal(callbackTypes.Length, parameter.typeCallback.Length);
        for (int i = 0; i < parameter.typeCallback.Length; i++)
            Assert.Equal(callbackTypes[i], parameter.typeCallback[i].type);
        Assert.Equal(typeCallbackPromise, parameter.typeCallbackPromise);
    }

    [Fact]
    public static void ParsingParameterTypeCallbackWithArrayAndNull() {
        TSParameter parameter = new();
        parameter.ParseType("(str: (string | null)[] | null) => void");

        Assert.Null(parameter.type);
        Assert.False(parameter.typeCallbackPromise);
        Assert.Equal(2, parameter.typeCallback.Length);
        Assert.Equal("void", parameter.typeCallback[1].type);

        Assert.Equal("string", parameter.typeCallback[0].type);
        Assert.True(parameter.typeCallback[0].typeNullable);
        Assert.True(parameter.typeCallback[0].array);
        Assert.True(parameter.typeCallback[0].arrayNullable);
    }

    [Fact]
    public static void ParsingParameterTypeCallbackNested() {
        TSParameter parameter = new();
        parameter.ParseType("(str: () => () => number) => () => void");

        Assert.Null(parameter.type);
        Assert.False(parameter.typeCallbackPromise);

        Assert.Equal(2, parameter.typeCallback.Length);
        
        {
            TSParameter nestedParameter = parameter.typeCallback[0];
            Assert.Null(nestedParameter.type);
            Assert.False(nestedParameter.typeCallbackPromise);

            Assert.Single(nestedParameter.typeCallback);

            {
                TSParameter nestednestedParameter = nestedParameter.typeCallback[0];
                Assert.Null(nestednestedParameter.type);
                Assert.False(nestednestedParameter.typeCallbackPromise);
                Assert.Single(nestednestedParameter.typeCallback);
                Assert.Equal("number", nestednestedParameter.typeCallback[0].type);
            }
        }

        {
            TSParameter nestedParameter = parameter.typeCallback[1];
            Assert.Null(nestedParameter.type);
            Assert.False(nestedParameter.typeCallbackPromise);
            Assert.Single(nestedParameter.typeCallback);
            Assert.Equal("void", nestedParameter.typeCallback[0].type);
        }
    }

    #endregion


    #region TSFunction

    [Theory]
    [InlineData("export declare function getCookies(): string;", "getCookies", "string", false, (string[])[])]
    [InlineData("export function asdf(): qwer;", "asdf", "qwer", false, (string[])[])]
    [InlineData("export function Test(a?: number): void;", "Test", "void", false, (string[])["a"])]
    [InlineData("export function defaultValueTest(index: [number, string]=[5, '']): void;", "defaultValueTest", "void", false, (string[])["index"])]
    [InlineData("export function longRunningTask(): Promise<void>;", "longRunningTask", "void", true, (string[])[])]
    [InlineData("export function longRunningTask2(): Promise<something>;", "longRunningTask2", "something", true, (string[])[])]
    [InlineData("export function Test(a: number, b: string, c:boolean): void;", "Test", "void", false, (string[])["a", "b", "c"])]

    [InlineData("export    declare  function   getCookies  (    )    :string;", "getCookies", "string", false, (string[])[])]
    [InlineData("export   function  defaultValueTest (   index:[number, string]   =      [5, '']    ): void;", "defaultValueTest", "void", false, (string[])["index"])]
    [InlineData("export function Test(a:number,b:       string,c:boolean)", "Test", "void", false, (string[])["a", "b", "c"])]
    public static void ParsingTSFunction(string input, string name, string returnType, bool promise, string[] paramterNames) {
        TSFunction function = TSFunction.ParseFunction(input)!;

        Assert.Equal(name, function.Name);
        Assert.Equal(returnType, function.ReturnType.type);
        Assert.Equal(promise, function.ReturnPromise);
        Assert.Equal(paramterNames.Length, function.ParameterList.Length);
        for (int i = 0; i < paramterNames.Length; i++)
            Assert.Equal(paramterNames[i], function.ParameterList[i].name);
    }

    [Theory]
    [InlineData("export function generic1<T>(): void;", "generic1", (string[])["T"])]
    [InlineData("export function generic3<Type, Key, Value>(): void;", "generic3", (string[])["Type", "Key", "Value"])]
    [InlineData("export function genericConstraint<Type extends HTMLElement>(): void;", "genericConstraint", (string[])["Type"])]
    [InlineData("export function genericConstraint<Type extends HTMLElement, Key>(): void;", "genericConstraint", (string[])["Type", "Key"])]
    [InlineData("export function genericConstraintGeneric<Type extends Map<K, V>>(): void;", "genericConstraintGeneric", (string[])["Type"])]
    [InlineData("export function genericKeyofConstraint<Type, Key extends keyof Type>(): void;", "genericKeyofConstraint", (string[])["Type", "Key"])]
    public static void ParsingTSGenericFunction(string input, string name, string[] generics) {
        TSFunction function = TSFunction.ParseFunction(input)!;

        Assert.Equal(name, function.Name);
        Assert.Equal(generics.Length, function.Generics.Length);
        for (int i = 0; i < function.Generics.Length; i++)
            Assert.Equal(generics[i], function.Generics[i]);
    }


    [Theory]
    [InlineData("export function example() { }", "example", (string[])[])]
    [InlineData("export   function  example2  (  ){}", "example2", (string[])[])]
    [InlineData("export function exampleVar(   myNumber  ){}", "exampleVar", (string[])["myNumber"])]
    [InlineData("export function exampleVar2(   myNumber  ,  str   ){}", "exampleVar2", (string[])["myNumber", "str"])]
    [InlineData("export function defaultValueTest(myName = 5) { }", "defaultValueTest", (string[])["myName"])]
    [InlineData("export function defaultValueTest(myName='wert') { }", "defaultValueTest", (string[])["myName"])]
    [InlineData("export function defaultValueTest(myName      =  3.21) { }", "defaultValueTest", (string[])["myName"])]
    public static void ParsingJSFunction(string input, string name, string[] paramterNames) {
        TSFunction function = TSFunction.ParseFunction(input)!;

        Assert.Equal(name, function.Name);
        Assert.Equal(new TSParameter() { name = "ReturnValue", type = "void" }, function.ReturnType);

        Assert.Equal(paramterNames.Length, function.ParameterList.Length);
        for (int i = 0; i < paramterNames.Length; i++)
            Assert.Equal(paramterNames[i], function.ParameterList[i].name);
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
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

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
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        Assert.Equal(summary, function.Summary);
        Assert.Equal(remarks, function.Remarks);
    }

    [Theory]
    [InlineData("/**@param*/\n", "", "", "")]
    [InlineData("/**@param integer - example*/\n", "", "example", "")]
    [InlineData("/**summaryExample @param integer example*/\n", "summaryExample", "example", "")]
    [InlineData("/**summaryExample\n * @param integer - example*/\n", "summaryExample", "example", "")]
    [InlineData("/**summaryExample\n * @param */\n", "summaryExample", "", "")]
    [InlineData("/**summaryExample\n * @param example*/\n", "summaryExample", "", "")]
    [InlineData("/**summaryExample\n * @param example @param integer ex*/\n", "summaryExample", "ex", "")]
    [InlineData("/**summaryExample\n * @returns nothing*/\n", "summaryExample", "", "nothing")]
    [InlineData("/**summaryExample\n * @param example @returns nothing*/\n", "summaryExample", "", "nothing")]
    [InlineData("/**summaryExample\n * @param integer a\n * @returns nothing*/\n", "summaryExample", "a", "nothing")]
    [InlineData("/**\n * a legit example\n * some more text\n *\n * @param integer - example\n */\n", "a legit example<br/>some more text", "example", "")]
    public static void ParsingSummaryWithParamTag(string input, string summary, string parameter, string returns) {
        const string FUNCTION_DECLARATION = "export function test(integer: number): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        Assert.Equal(summary, function.Summary);
        Assert.Equal(parameter, function.ParameterList[0].summary);
        Assert.Equal(returns, function.ReturnType.summary);
    }

    [Theory]
    [InlineData("/**@param {number}*/\n", "", "object", "", "void", "")]
    [InlineData("/**@param {number} integer - example*/\n", "", "number", "example", "void", "")]
    [InlineData("/**summaryExample @param {number} integer example*/\n", "summaryExample", "number", "example", "void", "")]
    [InlineData("/**summaryExample\n * @param {number} integer - example*/\n", "summaryExample", "number", "example", "void", "")]
    [InlineData("/**summaryExample\n * @param {  number   } integer - example*/\n", "summaryExample", "number", "example", "void", "")]
    [InlineData("/**summaryExample\n * @param {number} */\n", "summaryExample", "object", "", "void", "")]
    [InlineData("/**summaryExample\n * @param {number} example*/\n", "summaryExample", "object", "", "void", "")]
    [InlineData("/**summaryExample\n * @param {number} example @param integer ex*/\n", "summaryExample", "object", "ex", "void", "")]
    [InlineData("/**summaryExample\n * @returns {number} nothing*/\n", "summaryExample", "object", "", "number", "nothing")]
    [InlineData("/**summaryExample\n * @returns {Promise<number>} nothing*/\n", "summaryExample", "object", "", "number", "nothing")]
    [InlineData("/**summaryExample\n * @param {number} example @returns {string} nothing*/\n", "summaryExample", "object", "", "string", "nothing")]
    [InlineData("/**summaryExample\n * @param {number} integer a\n * @returns {string} nothing*/\n", "summaryExample", "number", "a", "string", "nothing")]
    [InlineData("/**\n * a legit example\n * some more text\n *\n * @param {number} integer - example\n */\n", "a legit example<br/>some more text", "number", "example", "void", "")]
    public static void ParsingSummaryWithTypes(string input, string summary, string paramterType, string parameterSummary, string returnType, string returnSummary) {
        const string FUNCTION_DECLARATION = "export function test(integer): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        Assert.Equal(summary, function.Summary);
        Assert.Equal(paramterType, function.ParameterList[0].type);
        Assert.Equal(parameterSummary, function.ParameterList[0].summary);
        Assert.Equal(returnType, function.ReturnType.type);
        Assert.Equal(returnSummary, function.ReturnType.summary);
    }

    [Theory]
    [InlineData("/** @param {string} str */\n", false)]
    [InlineData("/** @param {string} [str] */\n", true)]
    public static void ParsingSummaryOptional(string input, bool optional) {
        const string FUNCTION_DECLARATION = "export function test(str): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        Assert.Single(function.ParameterList);
        Assert.Equal("str", function.ParameterList[0].name);
        Assert.Equal("string", function.ParameterList[0].type);
        Assert.Equal(optional, function.ParameterList[0].optional);
    }


    [Fact]
    public static void ParsingTSFunctionWithCallback() {
        TSFunction function = TSFunction.ParseFunction("export function asdf(a: () => qwer): yxcv;")!;

        Assert.Equal("asdf", function.Name);
        Assert.Equal("yxcv", function.ReturnType.type);

        Assert.Single(function.ParameterList);
        {
            TSParameter parameter = function.ParameterList[0];
            Assert.Null(parameter.type);

            Assert.Single(parameter.typeCallback);
            {
                TSParameter callback = parameter.typeCallback[0];
                Assert.Equal("qwer", callback.type);
            }
        }

        Assert.True(function.HasCallback);
    }

    [Fact]
    public static void ParsingSummaryWithCallback() {
        const string input = "/**\n * @param {() => qwer} a\n * @returns {yxcv}\n */\n";
        TSFunction function = TSFunction.ParseFunction("export function asdf(a) { }")!;
        function.ParseSummary(input, input.Length);

        Assert.Equal("asdf", function.Name);
        Assert.Equal("yxcv", function.ReturnType.type);

        Assert.Single(function.ParameterList);
        {
            TSParameter parameter = function.ParameterList[0];
            Assert.Null(parameter.type);

            Assert.Single(parameter.typeCallback);
            {
                TSParameter callback = parameter.typeCallback[0];
                Assert.Equal("qwer", callback.type);
            }
        }

        Assert.True(function.HasCallback);
    }


    [Fact]
    public static void ParsingTSFunction_WrongStartReturnsNull() {
        TSFunction? result = TSFunction.ParseFunction("asdf");
        Assert.Null(result);
    }

    [Fact]
    public static void ParsingTSFunction_MissingOpenBracketError() {
        TSFunction? result = TSFunction.ParseFunction("export declare function Example[);");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "Example_Module", 1, result.Error.position);
        Assert.Equal("invalid file: 'Example_Module' at line 1: missing '(' after column 24 (the token that indicates the start of function parameters)", errorList[0].GetMessage());
    }

    [Fact]
    public static void ParsingTSFunction_MissingClosingGenericBracketError() {
        TSFunction? result = TSFunction.ParseFunction("export function Example<T]();");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "Example_Module", 1, result.Error.position);
        Assert.Equal("invalid file: 'Example_Module' at line 1: missing '>' after column 23 (the token that marks the end of generics)", errorList[0].GetMessage());
    }

    [Fact]
    public static void ParsingTSFunction_NoParameterEndError() {
        TSFunction? result = TSFunction.ParseFunction("export declare function Example(myNumber: number];");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "Example_Module", 1, result.Error.position);
        Assert.Equal("invalid file: 'Example_Module' at line 1: missing ')' after column 42 (the token that marks end of parameters)", errorList[0].GetMessage());
    }

    [Fact]
    public static void ParsingTSFunction_MissingPromiseEndingBracketError() {
        TSFunction? result = TSFunction.ParseFunction("export function Example(): Promise<   {");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "Example_Module", 1, result.Error.position);
        Assert.Equal("invalid file: 'Example_Module' at line 1: missing '>' after column 35 (the token that marks the end of generics)", errorList[0].GetMessage());
    }


    #endregion


    #region TSFile

    private const string RootFolder = "/CorePaserTestData";
    private const string MODULE = "TestModule";
    private const string MODULE_CONTENT = "export declare function Test(a: number, b: string): number;\n";
    private const string SCRIPT_CONTENT = "               function Test(a: number, b: string): number;\n";

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
        List<TSFunction> moduleFunctions = TSFunction.ParseFile(MODULE_CONTENT, isModule: true, null!, $"{RootFolder}/{MODULE}.d.ts");
        Assert.NotEmpty(moduleFunctions);
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
    [InlineData("test.d.ts")]
    [InlineData("/test.d.ts")]
    [InlineData("test.js")]
    [InlineData("/test.js")]
    public static void ParseMetaDataModulePath(string input) {
        TSModule tSModule = new(input, null, []);

        Assert.Equal($"/test.js", tSModule.URLPath);
        Assert.Equal("test", tSModule.Name);
    }


    [Fact]
    public static void PasingScript() {
        List<TSFunction> moduleFunctions = TSFunction.ParseFile(MODULE_CONTENT, isModule: false, null!, $"{RootFolder}/{MODULE}.d.ts");
        Assert.Empty(moduleFunctions);

        List<TSFunction> scriptFunctions = TSFunction.ParseFile(SCRIPT_CONTENT, isModule: false, null!, $"{RootFolder}/{MODULE}.d.ts");
        Assert.NotEmpty(scriptFunctions);
    }

    #endregion
}
