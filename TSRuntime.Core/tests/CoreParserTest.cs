using TSRuntime.Core.Parsing;
using Xunit;

namespace TSRuntime.Core.Tests;

public sealed class CoreParserTest {
    #region TSParamter

    [Theory]
    [InlineData("number", "number", false, false, false)]
    [InlineData("string", "string", false, false, false)]
    [InlineData("asdf", "asdf", false, false, false)]
    [InlineData("number | null", "number", true, false, false)]
    [InlineData("number | undefined", "number", true, false, false)]
    [InlineData("number[]", "number", false, true, false)]
    [InlineData("Array<number>", "number", false, true, false)]
    [InlineData("number[] | null", "number", false, true, true)]
    [InlineData("Array<number> | null", "number", false, true, true)]
    [InlineData("(number | null)[]", "number", true, true, false)]
    [InlineData("(number | null)[] | null", "number", true, true, true)]
    public void ParsingParameter_Works(string input, string type, bool typeNullable, bool array, bool arrayNullable) {
        TSParameter parameter = new();

        parameter.ParseType(input);

        Assert.Equal(type, parameter.Type);
        Assert.Equal(typeNullable, parameter.TypeNullable);
        Assert.Equal(array, parameter.Array);
        Assert.Equal(arrayNullable, parameter.ArrayNullable);
    }

    #endregion


    #region TSFunction

    [Fact]
    public void ParsingFunction_WrongStartRetunrsNull() {
        TSFunction? result = TSFunction.Parse("asdf");
        Assert.Null(result);
    }

    [Fact]
    public void ParsingFunction_RightStartButWrongSyntaxThrows() {
        Assert.Throws<Exception>(() => TSFunction.Parse("export declare function "));
    }

    [Theory]
    [InlineData("export declare function getCookies(): string;", "getCookies", "string", false)]
    [InlineData("export declare function asdf(): voidy;", "asdf", "voidy", false)]
    [InlineData("export declare function longRunningTask(): Promise<void>;", "longRunningTask", "void", true)]
    [InlineData("export declare function longRunningTask2(): Promise<something>;", "longRunningTask2", "something", true)]
    public void ParsingFunction_Works(string input, string name, string returnType, bool promise) {
        TSFunction function = TSFunction.Parse(input)!;

        Assert.Equal(name, function.Name);
        Assert.Equal(returnType, function.ReturnType.Type);
        Assert.Equal(promise, function.ReturnPromise);
    }

    #endregion


    #region TSModule

    private const string FOLDER = "TestFolder/";
    private const string MODULE = "CoreParserTest";
    private const string MODULE_FILE = $"{MODULE}.d.ts";

    [Fact]
    public async Task ParsingModule_WrongFilePathThrows() {
        await Assert.ThrowsAsync<ArgumentException>(() => TSModule.Parse("", string.Empty));
        await Assert.ThrowsAsync<FileNotFoundException>(() => TSModule.Parse($"#", string.Empty));
    }

    [Fact]
    public void ParsingModule_MetadataOnlyHasEmptyFunctionList() {
        TSModule module = new();
        module.ParseMetaData(MODULE_FILE, string.Empty);

        Assert.NotEqual(string.Empty, module.FilePath);
        Assert.NotEqual(string.Empty, module.RelativePath);
        Assert.NotEqual(string.Empty, module.ModulePath);
        Assert.NotEqual(string.Empty, module.ModuleName);
        Assert.Empty(module.FunctionList);
    }

    [Fact]
    public async Task ParsingModule_FunctionsOnlyHasEmptyMetaData() {
        TSModule module = new() {
            FilePath = MODULE_FILE
        };
        await module.ParseFunctions();

        Assert.Equal(MODULE_FILE, module.FilePath);
        Assert.Equal(string.Empty, module.RelativePath);
        Assert.Equal(string.Empty, module.ModulePath);
        Assert.Equal(string.Empty, module.ModuleName);
        Assert.NotEmpty(module.FunctionList);
    }

    [Fact]
    public async Task ParsingModule_Example() {
        TSModule module = await TSModule.Parse($"{FOLDER}{MODULE_FILE}", FOLDER);

        Assert.Equal($"{FOLDER}{MODULE_FILE}", module.FilePath);
        Assert.Equal(MODULE_FILE, module.RelativePath);
        Assert.Equal($"/{MODULE}.js", module.ModulePath);
        Assert.Equal(MODULE, module.ModuleName);
        Assert.Single(module.FunctionList);
    }

    #endregion


    #region TSSyntaxTree

    [Fact]
    public async Task SyntaxTree_ParseModulesParsesEvery_d_ts_File() {
        TSSyntaxTree syntaxTree = new();
        await syntaxTree.ParseModules("./");

        Assert.Equal(2, syntaxTree.ModuleList.Count);
        Assert.Empty(syntaxTree.FunctionList);
    }

    [Fact]
    public void SyntaxTree_ParseFunctionsThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => new TSSyntaxTree().ParseFunctions(string.Empty));
    }

    #endregion
}
