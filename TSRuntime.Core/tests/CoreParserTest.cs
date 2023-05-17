using TSRuntime.Core.Configs;
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
    [InlineData("readonly number[]", "number", false, true, false)]
    [InlineData("Array<number>", "number", false, true, false)]
    [InlineData("readonly Array<number>", "number", false, true, false)]
    [InlineData("number[] | null", "number", false, true, true)]
    [InlineData("Array<number> | null", "number", false, true, true)]
    [InlineData("(number | null)[]", "number", true, true, false)]
    [InlineData("(number | null)[] | null", "number", true, true, true)]
    [InlineData("readonly (number | null)[] | null", "number", true, true, true)]
    [InlineData("[number, string]", "[number, string]", false, false, false)]
    [InlineData("readonly [number, string]", "[number, string]", false, false, false)]
    public void ParsingParameter_Works(string input, string type, bool typeNullable, bool array, bool arrayNullable) {
        TSParameter parameter = new();

        parameter.ParseType(input);

        Assert.Equal(type, parameter.Type);
        Assert.Equal(typeNullable, parameter.TypeNullable);
        Assert.Equal(array, parameter.Array);
        Assert.Equal(arrayNullable, parameter.ArrayNullable);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("asdf", "asdf")]
    [InlineData("a?", "a")]
    [InlineData("asdf?", "asdf")]
    public void ParsingParameterName_Works(string input, string name) {
        TSParameter parameter = new();

        parameter.ParseName(input);

        Assert.Equal(name, parameter.Name);
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
    [InlineData("export declare function asdf(): qwer;", "asdf", "qwer", false)]
    [InlineData("export declare function Test(a?: number): void;", "Test", "void", false)]
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

    private const string RootFolder = "./CorePaserTestData";
    private const string MODULE = "TestModule";

    private const string NESTED_FOLDER = $"NestedFolder";
    private const string NESTED_MODULE = "NestedTestModule";


    [Fact]
    public async Task ParsingModule_WrongFilePathThrows() {
        await Assert.ThrowsAsync<ArgumentException>(() => TSModule.ParseWithRootFolder("", string.Empty));
        await Assert.ThrowsAsync<FileNotFoundException>(() => TSModule.ParseWithRootFolder($"#", string.Empty));
    }

    [Fact]
    public void ParsingModule_MetadataOnlyHasEmptyFunctionList() {
        TSModule module = new();
        module.ParseMetaDataRootFolder($"{RootFolder}/{MODULE}.d.ts", string.Empty);

        Assert.NotEqual(string.Empty, module.FilePath);
        Assert.NotEqual(string.Empty, module.ModulePath);
        Assert.NotEqual(string.Empty, module.ModuleName);
        Assert.Empty(module.FunctionList);
    }

    [Fact]
    public async Task ParsingModule_FunctionsOnlyHasEmptyMetaData() {
        const string filePath = $"{RootFolder}/{MODULE}.d.ts";
        TSModule module = new() {
            FilePath = filePath
        };
        await module.ParseFunctions();

        Assert.Equal(filePath, module.FilePath);
        Assert.Equal(string.Empty, module.ModulePath);
        Assert.Equal(string.Empty, module.ModuleName);
        Assert.NotEmpty(module.FunctionList);
    }

    [Fact]
    public async Task ParsingModuleWithRootFolder() {
        const string filePath = $"{RootFolder}/{MODULE}.d.ts";
        TSModule module = await TSModule.ParseWithRootFolder(filePath, RootFolder);

        Assert.Equal(filePath, module.FilePath);
        Assert.Equal($"/{MODULE}.js", module.ModulePath);
        Assert.Equal(MODULE, module.ModuleName);
        Assert.Single(module.FunctionList);
    }

    [Fact]
    public async Task ParsingModuleWithModulePath() {
        const string filePath = $"{RootFolder}/{MODULE}.d.ts";
        const string modulePath = "somePath";
        TSModule module = await TSModule.ParseWithModulePath(filePath, modulePath);

        Assert.Equal(filePath, module.FilePath);
        Assert.Equal($"/{modulePath}.js", module.ModulePath);
        Assert.Equal(modulePath, module.ModuleName);
        Assert.Single(module.FunctionList);
    }


    [Theory]
    [InlineData("test")]
    [InlineData("/test")]
    [InlineData("test.js")]
    [InlineData("/test.js")]
    public void ParseMetaDataModulePath(string input) {
        TSModule tSModule = new();
        tSModule.ParseMetaDataModulePath(string.Empty, input);

        Assert.Equal("/test.js", tSModule.ModulePath);
        Assert.Equal("test", tSModule.ModuleName);
    }

    #endregion


    #region TSStructureTree

    [Fact]
    public async Task StructureTree_ParseModules_ParsesEvery_d_ts_File_WhenParameterIsFolderString() {
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(RootFolder);

        Assert.Equal(2, structureTree.ModuleList.Count);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_ParsesEvery_d_ts_File_WhenParameterIsIncludeOnly() {
        DeclarationPath[] declarationPath = new DeclarationPath[1] {
            new DeclarationPath(RootFolder)
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Equal(2, structureTree.ModuleList.Count);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_ParsesNoExcludesFolder() {
        DeclarationPath[] declarationPath = new DeclarationPath[1] {
            new DeclarationPath(RootFolder) {
                Excludes = new string[1] { $"{RootFolder}/{NESTED_FOLDER}" }
            }
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Single(structureTree.ModuleList);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_ParsesNoExcludesFile() {
        DeclarationPath[] declarationPath = new DeclarationPath[1] {
            new DeclarationPath(RootFolder) {
                Excludes = new string[1] { $"{RootFolder}/{NESTED_FOLDER}/{NESTED_MODULE}.d.ts" }
            }
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Single(structureTree.ModuleList);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_ExcludesFolderAndFileOnly() {
        DeclarationPath[] declarationPath = new DeclarationPath[1] {
            new DeclarationPath(RootFolder) {
                Excludes = new string[1] { $"{RootFolder}/{NESTED_FOLDER[..^1]}" }
            }
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Equal(2, structureTree.ModuleList.Count);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_MultipleExcludes() {
        DeclarationPath[] declarationPath = new DeclarationPath[1] {
            new DeclarationPath(RootFolder) {
                Excludes = new string[2] { $"{RootFolder}/{NESTED_FOLDER}", $"{RootFolder}/{MODULE}.d.ts" }
            }
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Empty(structureTree.ModuleList);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_ParseMultipleFolders() {
        DeclarationPath[] declarationPath = new DeclarationPath[2] {
            new DeclarationPath(RootFolder),
            new DeclarationPath($"{RootFolder}/{NESTED_FOLDER}")
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Equal(3, structureTree.ModuleList.Count);
        Assert.Empty(structureTree.FunctionList);
    }


    [Fact]
    public async Task StructureTree_ParseModules_ParseModule_WhenPathIsFile() {
        DeclarationPath[] declarationPath = new DeclarationPath[1] {
            new DeclarationPath($"{RootFolder}/{MODULE}.d.ts") {
                FileModulePath = "somePath"
            }
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Single(structureTree.ModuleList);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_IgnoresExclude_WhenPathIsFile() {
        const string modulePath = $"{RootFolder}/{MODULE}.d.ts";
        DeclarationPath[] declarationPath = new DeclarationPath[1] {
            new DeclarationPath(modulePath) {
                Excludes = new string[1] { modulePath },
                FileModulePath = "somePath"
            }
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Single(structureTree.ModuleList);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_ThrowsDirectoryNotFoundException_WhenWrongModulePath() {
        DeclarationPath[] declarationPath = new DeclarationPath[1] { new("somePath") };
        TSStructureTree structureTree = new();

        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await structureTree.ParseModules(declarationPath));
    }

    [Fact]
    public async Task StructureTree_ParseModules_ParseMultipleModules() {
        DeclarationPath[] declarationPath = new DeclarationPath[2] {
            new DeclarationPath($"{RootFolder}/{MODULE}.d.ts") {
                FileModulePath = "somePath"
            },
            new DeclarationPath($"{RootFolder}/{NESTED_FOLDER}/{NESTED_MODULE}.d.ts") {
                FileModulePath = "somePath"
            }
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Equal(2, structureTree.ModuleList.Count);
        Assert.Empty(structureTree.FunctionList);
    }


    [Fact]
    public async Task StructureTree_ParseModules_FolderAndFilesWorks() {
        DeclarationPath[] declarationPath = new DeclarationPath[2] {
            new DeclarationPath($"{RootFolder}/{NESTED_FOLDER}"),
            new DeclarationPath($"{RootFolder}/{MODULE}.d.ts") {
                FileModulePath = "somePath"
            }
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Equal(2, structureTree.ModuleList.Count);
        Assert.Empty(structureTree.FunctionList);
    }

    [Fact]
    public async Task StructureTree_ParseModules_ExcludesAreScopedToSingle() {
        const string nestedFolderPath = $"{RootFolder}/{NESTED_FOLDER}";
        DeclarationPath[] declarationPath = new DeclarationPath[2] {
            new DeclarationPath(RootFolder) {
                Excludes = new string[1] { nestedFolderPath }
            },
            new DeclarationPath(nestedFolderPath)
        };
        TSStructureTree structureTree = new();

        await structureTree.ParseModules(declarationPath);

        Assert.Equal(2, structureTree.ModuleList.Count);
        Assert.Empty(structureTree.FunctionList);
    }


    [Fact]
    public void StructureTree_ParseFunctionsThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => new TSStructureTree().ParseFunctions(string.Empty));
    }

    #endregion
}
