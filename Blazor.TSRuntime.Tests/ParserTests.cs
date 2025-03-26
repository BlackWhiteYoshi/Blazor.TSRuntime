using Microsoft.CodeAnalysis;
using TSRuntime.Parsing;

namespace TSRuntime.Tests;

public sealed class ParserTests {
    #region TSParamter

    [Test]
    [Arguments("", "", false)]
    [Arguments("a", "a", false)]
    [Arguments("asdf", "asdf", false)]
    [Arguments("a?", "a", true)]
    [Arguments("asdf?", "asdf", true)]
    public async ValueTask ParsingParameterName(string input, string name, bool optional) {
        TSParameter parameter = new();
        parameter.ParseName(input);

        await Assert.That(parameter.name).IsEqualTo(name);
        await Assert.That(parameter.optional).IsEqualTo(optional);
    }

    [Test]
    [Arguments("number", "number", false, false, false, false)]
    [Arguments("string", "string", false, false, false, false)]
    [Arguments("asdf", "asdf", false, false, false, false)]

    [Arguments("number | null", "number", true, false, false, false)]
    [Arguments("number | undefined", "number", false, false, false, true)]
    [Arguments("number | null | undefined", "number", true, false, false, true)]
    [Arguments("number | undefined | null", "number", true, false, false, true)]
    [Arguments("null | undefined | number", "number", true, false, false, true)]
    [Arguments("undefined | null | number", "number", true, false, false, true)]
    [Arguments("null | number | undefined", "number", true, false, false, true)]
    [Arguments("undefined | number | null", "number", true, false, false, true)]


    [Arguments("number[]", "number", false, true, false, false)]
    [Arguments("readonly number[]", "number", false, true, false, false)]
    [Arguments("Array<number>", "number", false, true, false, false)]
    [Arguments("readonly Array<number>", "number", false, true, false, false)]
    [Arguments("number[] | null", "number", false, true, true, false)]
    [Arguments("number[] | undefined", "number", false, true, false, true)]
    [Arguments("number[] | null | undefined", "number", false, true, true, true)]
    [Arguments("Array<number> | null", "number", false, true, true, false)]
    [Arguments("Array<number> | undefined", "number", false, true, false, true)]
    [Arguments("Array<number> | null | undefined", "number", false, true, true, true)]

    [Arguments("(number | null)[]", "number", true, true, false, false)]
    [Arguments("(number | undefined)[]", "number", true, true, false, false)]
    [Arguments("(number | null | undefined)[]", "number", true, true, false, false)]
    [Arguments("(number | null)[] | null", "number", true, true, true, false)]
    [Arguments("(number | undefined)[] | undefined", "number", true, true, false, true)]
    [Arguments("(number | null | undefined)[] | null | undefined", "number", true, true, true, true)]
    [Arguments("readonly (number | null)[] | null", "number", true, true, true, false)]
    [Arguments("readonly (number | undefined)[] | undefined", "number", true, true, false, true)]
    [Arguments("readonly (number | null | undefined)[] | null | undefined", "number", true, true, true, true)]

    [Arguments("[number, string]", "[number, string]", false, false, false, false)]
    [Arguments("readonly [number, string]", "[number, string]", false, false, false, false)]

    [Arguments("number  |    null|undefined", "number", true, false, false, true)]
    [Arguments("readonly     number    [   ]", "number", false, true, false, false)]
    [Arguments("readonly   Array<   number  >", "number", false, true, false, false)]
    [Arguments("(   number    |null|     undefined     )   [ ]|null|undefined", "number", true, true, true, true)]
    public async ValueTask ParsingParameterType(string input, string type, bool typeNullable, bool array, bool arrayNullable, bool optional) {
        TSParameter parameter = new();
        parameter.ParseType(input);

        await Assert.That(parameter.type).IsEqualTo(type);
        await Assert.That(parameter.typeNullable).IsEqualTo(typeNullable);
        await Assert.That(parameter.array).IsEqualTo(array);
        await Assert.That(parameter.arrayNullable).IsEqualTo(arrayNullable);
        await Assert.That(parameter.optional).IsEqualTo(optional);
    }

    [Test]
    [Arguments("(str: string) => number", (string[])["string", "number"], false)]
    [Arguments("() => void", (string[])["void"], false)]
    [Arguments("(str: string) => Promise<number>", (string[])["string", "number"], true)]
    [Arguments("(a: boolean, b: number) => void", (string[])["boolean", "number", "void"], false)]
    [Arguments("(a:boolean,b:number)=>void", (string[])["boolean", "number", "void"], false)]
    [Arguments("(   a :      boolean ,  b  :   number    ) =>    void", (string[])["boolean", "number", "void"], false)]
    public async ValueTask ParsingParameterTypeCallback(string input, string[] callbackTypes, bool typeCallbackPromise) {
        TSParameter parameter = new();
        parameter.ParseType(input);


        await Assert.That(parameter.type).IsNull();
        await Assert.That(parameter.typeCallback.Length).IsEqualTo(callbackTypes.Length);
        for (int i = 0; i < parameter.typeCallback.Length; i++)
            await Assert.That(parameter.typeCallback[i].type).IsEqualTo(callbackTypes[i]);
        await Assert.That(parameter.typeCallbackPromise).IsEqualTo(typeCallbackPromise);
    }

    [Test]
    public async ValueTask ParsingParameterTypeCallbackWithArrayAndNull() {
        TSParameter parameter = new();
        parameter.ParseType("(str: (string | null)[] | null) => void");

        await Assert.That(parameter.type).IsNull();
        await Assert.That(parameter.typeCallbackPromise).IsFalse();
        await Assert.That(parameter.typeCallback.Length).IsEqualTo(2);
        await Assert.That(parameter.typeCallback[1].type).IsEqualTo("void");

        await Assert.That(parameter.typeCallback[0].type).IsEqualTo("string");
        await Assert.That(parameter.typeCallback[0].typeNullable).IsTrue();
        await Assert.That(parameter.typeCallback[0].array).IsTrue();
        await Assert.That(parameter.typeCallback[0].arrayNullable).IsTrue();
    }

    [Test]
    public async ValueTask ParsingParameterTypeCallbackNested() {
        TSParameter parameter = new();
        parameter.ParseType("(str: () => () => number) => () => void");

        await Assert.That(parameter.type).IsNull();
        await Assert.That(parameter.typeCallbackPromise).IsFalse();

        await Assert.That(parameter.typeCallback.Length).IsEqualTo(2);

        {
            TSParameter nestedParameter = parameter.typeCallback[0];
            await Assert.That(nestedParameter.type).IsNull();
            await Assert.That(nestedParameter.typeCallbackPromise).IsFalse();

            await Assert.That(nestedParameter.typeCallback).HasSingleItem();

            {
                TSParameter nestednestedParameter = nestedParameter.typeCallback[0];
                await Assert.That(nestednestedParameter.type).IsNull();
                await Assert.That(nestednestedParameter.typeCallbackPromise).IsFalse();
                await Assert.That(nestednestedParameter.typeCallback).HasSingleItem();
                await Assert.That(nestednestedParameter.typeCallback[0].type).IsEqualTo("number");
            }
        }

        {
            TSParameter nestedParameter = parameter.typeCallback[1];
            await Assert.That(nestedParameter.type).IsNull();
            await Assert.That(nestedParameter.typeCallbackPromise).IsFalse();
            await Assert.That(nestedParameter.typeCallback).HasSingleItem();
            await Assert.That(nestedParameter.typeCallback[0].type).IsEqualTo("void");
        }
    }

    #endregion


    #region TSFunction

    [Test]
    [Arguments("export declare function getCookies(): string;", "getCookies", "string", false, (string[])[])]
    [Arguments("export function asdf(): qwer;", "asdf", "qwer", false, (string[])[])]
    [Arguments("export function Test(a?: number): void;", "Test", "void", false, (string[])["a"])]
    [Arguments("export function defaultValueTest(index: [number, string]=[5, '']): void;", "defaultValueTest", "void", false, (string[])["index"])]
    [Arguments("export function longRunningTask(): Promise<void>;", "longRunningTask", "void", true, (string[])[])]
    [Arguments("export function longRunningTask2(): Promise<something>;", "longRunningTask2", "something", true, (string[])[])]
    [Arguments("export function Test(a: number, b: string, c:boolean): void;", "Test", "void", false, (string[])["a", "b", "c"])]

    [Arguments("export    declare  function   getCookies  (    )    :string;", "getCookies", "string", false, (string[])[])]
    [Arguments("export   function  defaultValueTest (   index:[number, string]   =      [5, '']    ): void;", "defaultValueTest", "void", false, (string[])["index"])]
    [Arguments("export function Test(a:number,b:       string,c:boolean)", "Test", "void", false, (string[])["a", "b", "c"])]
    public async ValueTask ParsingTSFunction(string input, string name, string returnType, bool promise, string[] paramterNames) {
        TSFunction function = TSFunction.ParseFunction(input)!;

        await Assert.That(function.Name).IsEqualTo(name);
        await Assert.That(function.ReturnType.type).IsEqualTo(returnType);
        await Assert.That(function.ReturnPromise).IsEqualTo(promise);
        await Assert.That(function.ParameterList.Length).IsEqualTo(paramterNames.Length);
        for (int i = 0; i < paramterNames.Length; i++)
            await Assert.That(function.ParameterList[i].name).IsEqualTo(paramterNames[i]);
    }

    [Test]
    [Arguments("export function generic1<T>(): void;", "generic1", (string[])["T"])]
    [Arguments("export function generic3<Type, Key, Value>(): void;", "generic3", (string[])["Type", "Key", "Value"])]
    [Arguments("export function genericConstraint<Type extends HTMLElement>(): void;", "genericConstraint", (string[])["Type"])]
    [Arguments("export function genericConstraint<Type extends HTMLElement, Key>(): void;", "genericConstraint", (string[])["Type", "Key"])]
    [Arguments("export function genericConstraintGeneric<Type extends Map<K, V>>(): void;", "genericConstraintGeneric", (string[])["Type"])]
    [Arguments("export function genericKeyofConstraint<Type, Key extends keyof Type>(): void;", "genericKeyofConstraint", (string[])["Type", "Key"])]
    public async ValueTask ParsingTSGenericFunction(string input, string name, string[] generics) {
        TSFunction function = TSFunction.ParseFunction(input)!;

        await Assert.That(function.Name).IsEqualTo(name);
        await Assert.That(function.Generics.Length).IsEqualTo(generics.Length);
        for (int i = 0; i < function.Generics.Length; i++)
            await Assert.That(function.Generics[i].type).IsEqualTo(generics[i]);
    }


    [Test]
    [Arguments("export function example() { }", "example", (string[])[])]
    [Arguments("export   function  example2  (  ){}", "example2", (string[])[])]
    [Arguments("export function exampleVar(   myNumber  ){}", "exampleVar", (string[])["myNumber"])]
    [Arguments("export function exampleVar2(   myNumber  ,  str   ){}", "exampleVar2", (string[])["myNumber", "str"])]
    [Arguments("export function defaultValueTest(myName = 5) { }", "defaultValueTest", (string[])["myName"])]
    [Arguments("export function defaultValueTest(myName='wert') { }", "defaultValueTest", (string[])["myName"])]
    [Arguments("export function defaultValueTest(myName      =  3.21) { }", "defaultValueTest", (string[])["myName"])]
    public async ValueTask ParsingJSFunction(string input, string name, string[] paramterNames) {
        TSFunction function = TSFunction.ParseFunction(input)!;

        await Assert.That(function.Name).IsEqualTo(name);
        await Assert.That(function.ReturnType).IsEqualTo(new TSParameter() { name = "ReturnValue", type = "void" });

        await Assert.That(function.ParameterList.Length).IsEqualTo(paramterNames.Length);
        for (int i = 0; i < paramterNames.Length; i++)
            await Assert.That(function.ParameterList[i].name).IsEqualTo(paramterNames[i]);
    }


    [Test]
    [Arguments("\n\n", "")]
    [Arguments("/**/\n", "")]
    [Arguments("/***/\n", "")]
    [Arguments("/**example*/\n", "example")]
    [Arguments("/**   example  */\n", "example")]
    [Arguments("/**  * example  */\n", "example")]
    [Arguments("/**  ** example  */\n", "* example")]
    [Arguments("/**\n*\n* example  */\n", "example")]
    [Arguments("/** example\n */\n", "example")]
    [Arguments("/**\n * a legit example\n * some more text\n *\n * some note\n */\n", "a legit example<br/>some more text<br/><br/>some note")]
    public async ValueTask ParsingSummary(string input, string summary) {
        const string FUNCTION_DECLARATION = "export function test(): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        await Assert.That(function.Summary).IsEqualTo(summary);
    }

    [Test]
    [Arguments("/**@remarks*/\n", "", "")]
    [Arguments("/**@remarks example*/\n", "", "example")]
    [Arguments("/**summaryExample @remarks example*/\n", "summaryExample", "example")]
    [Arguments("/**summaryExample\n * @remarks example*/\n", "summaryExample", "example")]
    [Arguments("/**summaryExample\n * @unkownT example*/\n", "summaryExample", "")]
    [Arguments("/**summaryExample\n * @unkownT example@remarks ex*/\n", "summaryExample", "ex")]
    [Arguments("/**summaryExample\n * @unkownT @remarks ex*/\n", "summaryExample", "ex")]
    [Arguments("/**\n * a legit example\n * some more text\n *\n * @remarks example\n */\n", "a legit example<br/>some more text", "example")]
    public async ValueTask ParsingSummaryWithTag(string input, string summary, string remarks) {
        const string FUNCTION_DECLARATION = "export function test(): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        await Assert.That(function.Summary).IsEqualTo(summary);
        await Assert.That(function.Remarks).IsEqualTo(remarks);
    }

    [Test]
    [Arguments("/**@param*/\n", "", "", "")]
    [Arguments("/**@param integer - example*/\n", "", "example", "")]
    [Arguments("/**summaryExample @param integer example*/\n", "summaryExample", "example", "")]
    [Arguments("/**summaryExample\n * @param integer - example*/\n", "summaryExample", "example", "")]
    [Arguments("/**summaryExample\n * @param */\n", "summaryExample", "", "")]
    [Arguments("/**summaryExample\n * @param example*/\n", "summaryExample", "", "")]
    [Arguments("/**summaryExample\n * @param example @param integer ex*/\n", "summaryExample", "ex", "")]
    [Arguments("/**summaryExample\n * @returns nothing*/\n", "summaryExample", "", "nothing")]
    [Arguments("/**summaryExample\n * @param example @returns nothing*/\n", "summaryExample", "", "nothing")]
    [Arguments("/**summaryExample\n * @param integer a\n * @returns nothing*/\n", "summaryExample", "a", "nothing")]
    [Arguments("/**\n * a legit example\n * some more text\n *\n * @param integer - example\n */\n", "a legit example<br/>some more text", "example", "")]
    public async ValueTask ParsingSummaryWithParamTag(string input, string summary, string parameter, string returns) {
        const string FUNCTION_DECLARATION = "export function test(integer: number): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        await Assert.That(function.Summary).IsEqualTo(summary);
        await Assert.That(function.ParameterList[0].summary).IsEqualTo(parameter);
        await Assert.That(function.ReturnType.summary).IsEqualTo(returns);
    }

    [Test]
    [Arguments("/**@typeparam missing type example*/\n", "", false, "", "")]
    [Arguments("/**summaryExample\n * @typeparam missing type example*/\n", "summaryExample", false, "", "")]
    [Arguments("/**summaryExample\n * @typeparam {a}*/\n", "summaryExample", true, "a", "")]
    [Arguments("/**summaryExample\n * @typeparam {T}*/\n", "summaryExample", true, "T", "")]
    [Arguments("/**summaryExample\n * @typeparam {A} upper letter A*/\n", "summaryExample", true, "A", "upper letter A")]
    public async ValueTask ParsingSummaryWithTypeParamTag(string input, string summary, bool success, string typeParameter, string typeParameterDescription) {
        const string FUNCTION_DECLARATION = "export function test<T>(): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        await Assert.That(function.Summary).IsEqualTo(summary);

        if (success) {
            await Assert.That(function.Generics.Length).IsEqualTo(2);
            await Assert.That(function.Generics[1].type).IsEqualTo(typeParameter);
            await Assert.That(function.Generics[1].description).IsEqualTo(typeParameterDescription);
        }
        else
            await Assert.That(function.Generics).HasSingleItem();
    }

    [Test]
    [Arguments("/**@param {number}*/\n", "", "object", "", "void", "")]
    [Arguments("/**@param {number} integer - example*/\n", "", "number", "example", "void", "")]
    [Arguments("/**summaryExample @param {number} integer example*/\n", "summaryExample", "number", "example", "void", "")]
    [Arguments("/**summaryExample\n * @param {number} integer - example*/\n", "summaryExample", "number", "example", "void", "")]
    [Arguments("/**summaryExample\n * @param {  number   } integer - example*/\n", "summaryExample", "number", "example", "void", "")]
    [Arguments("/**summaryExample\n * @param {number} */\n", "summaryExample", "object", "", "void", "")]
    [Arguments("/**summaryExample\n * @param {number} example*/\n", "summaryExample", "object", "", "void", "")]
    [Arguments("/**summaryExample\n * @param {number} example @param integer ex*/\n", "summaryExample", "object", "ex", "void", "")]
    [Arguments("/**summaryExample\n * @returns {number} nothing*/\n", "summaryExample", "object", "", "number", "nothing")]
    [Arguments("/**summaryExample\n * @returns {Promise<number>} nothing*/\n", "summaryExample", "object", "", "number", "nothing")]
    [Arguments("/**summaryExample\n * @param {number} example @returns {string} nothing*/\n", "summaryExample", "object", "", "string", "nothing")]
    [Arguments("/**summaryExample\n * @param {number} integer a\n * @returns {string} nothing*/\n", "summaryExample", "number", "a", "string", "nothing")]
    [Arguments("/**\n * a legit example\n * some more text\n *\n * @param {number} integer - example\n */\n", "a legit example<br/>some more text", "number", "example", "void", "")]
    public async ValueTask ParsingSummaryWithTypes(string input, string summary, string paramterType, string parameterSummary, string returnType, string returnSummary) {
        const string FUNCTION_DECLARATION = "export function test(integer): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        await Assert.That(function.Summary).IsEqualTo(summary);
        await Assert.That(function.ParameterList[0].type).IsEqualTo(paramterType);
        await Assert.That(function.ParameterList[0].summary).IsEqualTo(parameterSummary);
        await Assert.That(function.ReturnType.type).IsEqualTo(returnType);
        await Assert.That(function.ReturnType.summary).IsEqualTo(returnSummary);
    }

    [Test]
    [Arguments("/** @param {string} str */\n", false)]
    [Arguments("/** @param {string} [str] */\n", true)]
    public async ValueTask ParsingSummaryOptional(string input, bool optional) {
        const string FUNCTION_DECLARATION = "export function test(str): void;";
        TSFunction function = TSFunction.ParseFunction(FUNCTION_DECLARATION)!;
        function.ParseSummary($"{input}{FUNCTION_DECLARATION}", input.Length);

        await Assert.That(function.ParameterList).HasSingleItem();
        await Assert.That(function.ParameterList[0].name).IsEqualTo("str");
        await Assert.That(function.ParameterList[0].type).IsEqualTo("string");
        await Assert.That(function.ParameterList[0].optional).IsEqualTo(optional);
    }


    [Test]
    public async ValueTask ParsingTSFunctionWithCallback() {
        TSFunction function = TSFunction.ParseFunction("export function asdf(a: () => qwer): yxcv;")!;

        await Assert.That(function.Name).IsEqualTo("asdf");
        await Assert.That(function.ReturnType.type).IsEqualTo("yxcv");

        await Assert.That(function.ParameterList).HasSingleItem();
        {
            TSParameter parameter = function.ParameterList[0];
            await Assert.That(parameter.type).IsNull();

            await Assert.That(parameter.typeCallback).HasSingleItem();
            {
                TSParameter callback = parameter.typeCallback[0];
                await Assert.That(callback.type).IsEqualTo("qwer");
            }
        }

        await Assert.That(function.HasCallback).IsTrue();
    }

    [Test]
    public async ValueTask ParsingSummaryWithCallback() {
        const string input = "/**\n * @param {() => qwer} a\n * @returns {yxcv}\n */\n";
        TSFunction function = TSFunction.ParseFunction("export function asdf(a) { }")!;
        function.ParseSummary(input, input.Length);

        await Assert.That(function.Name).IsEqualTo("asdf");
        await Assert.That(function.ReturnType.type).IsEqualTo("yxcv");

        await Assert.That(function.ParameterList).HasSingleItem();
        {
            TSParameter parameter = function.ParameterList[0];
            await Assert.That(parameter.type).IsNull();

            await Assert.That(parameter.typeCallback).HasSingleItem();
            {
                TSParameter callback = parameter.typeCallback[0];
                await Assert.That(callback.type).IsEqualTo("qwer");
            }
        }

        await Assert.That(function.HasCallback).IsTrue();
    }


    [Test]
    public async ValueTask ParsingTSFunction_WrongStartReturnsNull() {
        TSFunction? result = TSFunction.ParseFunction("asdf");
        await Assert.That(result).IsNull();
    }

    [Test]
    public async ValueTask ParsingTSFunction_MissingOpenBracketError() {
        TSFunction? result = TSFunction.ParseFunction("export declare function Example[);");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "Example_Module", 1, result.Error.position);
        await Assert.That(errorList[0].GetMessage()).IsEqualTo("invalid file: 'Example_Module' at line 1: missing '(' after column 24 (the token that indicates the start of function parameters)");
    }

    [Test]
    public async ValueTask ParsingTSFunction_MissingClosingGenericBracketError() {
        TSFunction? result = TSFunction.ParseFunction("export function Example<T]();");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "Example_Module", 1, result.Error.position);
        await Assert.That(errorList[0].GetMessage()).IsEqualTo("invalid file: 'Example_Module' at line 1: missing '>' after column 23 (the token that marks the end of generics)");
    }

    [Test]
    public async ValueTask ParsingTSFunction_NoParameterEndError() {
        TSFunction? result = TSFunction.ParseFunction("export declare function Example(myNumber: number];");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "Example_Module", 1, result.Error.position);
        await Assert.That(errorList[0].GetMessage()).IsEqualTo("invalid file: 'Example_Module' at line 1: missing ')' after column 42 (the token that marks end of parameters)");
    }

    [Test]
    public async ValueTask ParsingTSFunction_MissingPromiseEndingBracketError() {
        TSFunction? result = TSFunction.ParseFunction("export function Example(): Promise<   {");

        List<Diagnostic> errorList = [];
        errorList.AddFunctionParseError(result!.Error.descriptor!, "Example_Module", 1, result.Error.position);
        await Assert.That(errorList[0].GetMessage()).IsEqualTo("invalid file: 'Example_Module' at line 1: missing '>' after column 35 (the token that marks the end of generics)");
    }


    #endregion


    #region TSFile

    private const string RootFolder = "/CorePaserTestData";
    private const string MODULE = "TestModule";
    private const string MODULE_CONTENT = "export declare function Test(a: number, b: string): number;\n";
    private const string SCRIPT_CONTENT = "               function Test(a: number, b: string): number;\n";

    [Test]
    public async ValueTask ParsingModule_MetadataOnlyHasEmptyFunctionList() {
        TSModule module = new($"{RootFolder}/{MODULE}.d.ts", null, []);

        await Assert.That(module.FilePath).IsNotEqualTo(string.Empty);
        await Assert.That(module.URLPath).IsNotEqualTo(string.Empty);
        await Assert.That(module.Name).IsNotEqualTo(string.Empty);
        await Assert.That(module.FunctionList).IsEmpty();
    }

    [Test]
    public async ValueTask ParsingModule_FunctionsOnlyHasEmptyMetaData() {
        List<TSFunction> moduleFunctions = TSFunction.ParseFile(MODULE_CONTENT, isModule: true, null!, $"{RootFolder}/{MODULE}.d.ts");
        await Assert.That(moduleFunctions).IsNotEmpty();
    }

    [Test]
    public async ValueTask ParsingModuleWithModulePath() {
        const string modulePath = "somePath";
        TSModule module = new($"{RootFolder}/{MODULE}.d.ts", $"{modulePath}.js", []);

        await Assert.That(module.FilePath).IsEqualTo($"{RootFolder}/{MODULE}.d.ts");
        await Assert.That(module.URLPath).IsEqualTo($"/{modulePath}.js");
        await Assert.That(module.Name).IsEqualTo(modulePath);
    }

    [Test]
    [Arguments("test.d.ts")]
    [Arguments("/test.d.ts")]
    [Arguments("test.js")]
    [Arguments("/test.js")]
    public async ValueTask ParseMetaDataModulePath(string input) {
        TSModule tSModule = new(input, null, []);

        await Assert.That(tSModule.URLPath).IsEqualTo($"/test.js");
        await Assert.That(tSModule.Name).IsEqualTo("test");
    }


    [Test]
    public async ValueTask PasingScript() {
        List<TSFunction> moduleFunctions = TSFunction.ParseFile(MODULE_CONTENT, isModule: false, null!, $"{RootFolder}/{MODULE}.d.ts");
        await Assert.That(moduleFunctions).IsEmpty();

        List<TSFunction> scriptFunctions = TSFunction.ParseFile(SCRIPT_CONTENT, isModule: false, null!, $"{RootFolder}/{MODULE}.d.ts");
        await Assert.That(scriptFunctions).IsNotEmpty();
    }

    #endregion
}
