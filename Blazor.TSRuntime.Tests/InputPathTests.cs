using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using TSRuntime.Configs;

namespace TSRuntime.Tests;

public static class InputPathTests {
    private static readonly (string path, string content) testModule = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/TestModule.d.ts", "export declare function Test(a: number, b: string): number;\n");
    private static readonly (string path, string content) nestedTestModule = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/NestedFolder/NestedTestModule.d.ts", "export declare function NestedTest(): void;\n");


    [Fact]
    public static void ParsesEvery_d_ts_File_WhenInputPathEmpty() {
        const string jsonConfig = """
            {
                "input path": "",
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs", "ITSRuntime_NestedTestModule.g.cs"], hintNames);
    }

    [Fact]
    public static void ParsesEvery_d_ts_File_WhenInputPathSlash() {
        const string jsonConfig = """
            {
                "input path": "/",
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs", "ITSRuntime_NestedTestModule.g.cs"], hintNames);
    }

    [Fact]
    public static void FilterExcludesFolder() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "",
                    "excludes": ""
                },
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs"], hintNames);
    }

    [Fact]
    public static void FilterExcludesFile() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "",
                    "excludes": "/TestModule.d.ts"
                },
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_NestedTestModule.g.cs"], hintNames);
    }

    [Fact]
    public static void FilterExcludesFileAndFolder() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "",
                    "excludes": ["/TestModule.d.ts", "/NestedFolder/"]
                },
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs"], hintNames);
    }

    [Fact]
    public static void FilterExcludesFileWithoutLeadingSlash() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "",
                    "excludes": "TestModule.d.ts"
                },
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_NestedTestModule.g.cs"], hintNames);
    }

    [Fact]
    public static void MultipleIncludes() {
        const string jsonConfig = """
            {
                "input path": ["/TestModule.d.ts", "/NestedFolder/NestedTestModule.d.ts"],
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs", "ITSRuntime_NestedTestModule.g.cs"], hintNames);
    }

    [Fact]
    public static void ExcludeFolderDoesNotExcludeFile() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "/TestModule.d.ts",
                    "excludes": ["/TestModule"]
                },
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs"], hintNames);
    }

    [Fact]
    public static void ExcludesAreScopedToSingle() {
        const string jsonConfig = """
            {
                "input path": [{
                    "include": "",
                    "excludes": ["/TestModule.d.ts", "/NestedFolder/NestedTestModule.d.ts"]
                },
                ""],
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs", "ITSRuntime_NestedTestModule.g.cs"], hintNames);
    }

    [Fact]
    public static void WrongFilePathIsIgnored() {
        const string jsonConfig = """
            {
                "input path": "/TModule.d.ts",
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        Assert.Equal(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs"], hintNames);
    }

    [Fact]
    public static void ModulePath() {
        const string jsonConfig = """
            {
                "input path":  {
                    "include": "/TestModule.d.ts",
                    "module path" : "/site.js"
                },
                "service extension": false
            }
            """;
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out _);
        string tsRuntime = result[0];

        Assert.Contains("""_ => siteModule = jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationTokenSource.Token, "/site.js").AsTask()""", tsRuntime);
    }

    [Fact]
    public static void ModulePathEmpty() {
        const string jsonConfig = """
            {
                "input path":  {
                    "include": "/TestModule.d.ts",
                    "module path" : ""
                },
                "service extension": false
            }
            """;
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out _);
        string tsRuntime = result[0];

        Assert.Contains("""_ => Module = jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationTokenSource.Token, "/").AsTask()""", tsRuntime);
    }

    [Fact]
    public static void ModulePathOnlySlash() {
        const string jsonConfig = """
            {
                "input path":  {
                    "include": "/TestModule.d.ts",
                    "module path" : "/"
                },
                "service extension": false
            }
            """;
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out _);
        string tsRuntime = result[0];

        Assert.Contains("""_ => Module = jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationTokenSource.Token, "/").AsTask()""", tsRuntime);
    }

    [Fact]
    public static void IncludeFolderWithModulePath_HasConflictingHintNames() {
        const string jsonConfig = """
            {
                "input path":  {
                    "include": "",
                    "module path" : "/site.js"
                },
                "service extension": false
            }
            """;
        _ = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);

        Assert.Single(diagnostics);
    }
}
