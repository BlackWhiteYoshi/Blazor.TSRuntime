using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using TSRuntime.Configs;

namespace TSRuntime.Tests;

public sealed class InputPathTests {
    private static readonly (string path, string content) testModule = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/TestModule.d.ts", "export declare function Test(a: number, b: string): number;\n");
    private static readonly (string path, string content) nestedTestModule = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/NestedFolder/NestedTestModule.d.ts", "export declare function NestedTest(): void;\n");


    [Test]
    public async ValueTask ParsesEvery_d_ts_File_WhenInputPathEmpty() {
        const string jsonConfig = """
            {
                "input path": "",
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs", "ITSRuntime_NestedTestModule.g.cs"]);
    }

    [Test]
    public async ValueTask ParsesEvery_d_ts_File_WhenInputPathSlash() {
        const string jsonConfig = """
            {
                "input path": "/",
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs", "ITSRuntime_NestedTestModule.g.cs"]);
    }

    [Test]
    public async ValueTask FilterExcludesFolder() {
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

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs"]);
    }

    [Test]
    public async ValueTask FilterExcludesFile() {
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

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_NestedTestModule.g.cs"]);
    }

    [Test]
    public async ValueTask FilterExcludesFileAndFolder() {
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

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs"]);
    }

    [Test]
    public async ValueTask MultipleIncludes() {
        const string jsonConfig = """
            {
                "input path": ["/TestModule.d.ts", "/NestedFolder/NestedTestModule.d.ts"],
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs", "ITSRuntime_NestedTestModule.g.cs"]);
    }

    [Test]
    public async ValueTask ExcludeFolderDoesNotExcludeFile() {
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

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs"]);
    }

    [Test]
    public async ValueTask ExcludesAreScopedToSingle() {
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

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs", "ITSRuntime_TestModule.g.cs", "ITSRuntime_NestedTestModule.g.cs"]);
    }

    [Test]
    public async ValueTask WrongFilePathIsIgnored() {
        const string jsonConfig = """
            {
                "input path": "/TModule.d.ts",
                "service extension": false
            }
            """;
        ImmutableArray<GeneratedSourceResult> result = jsonConfig.GenerateSourceResult([testModule, nestedTestModule], out _, out _);
        IEnumerable<string> hintNames = result.Select((GeneratedSourceResult source) => source.HintName);

        await Assert.That(hintNames).IsEquivalentTo(["TSRuntime.g.cs", "ITSRuntime_Core.g.cs"]);
    }

    [Test]
    public async ValueTask ModulePath() {
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

        await Assert.That(tsRuntime).Contains("""_ => siteModule = jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationTokenSource.Token, "/site.js").AsTask()""");
    }

    [Test]
    public async ValueTask ModulePathEmpty() {
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

        await Assert.That(tsRuntime).Contains("""_ => Module = jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationTokenSource.Token, "/").AsTask()""");
    }

    [Test]
    public async ValueTask ModulePathOnlySlash() {
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

        await Assert.That(tsRuntime).Contains("""_ => Module = jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationTokenSource.Token, "/").AsTask()""");
    }

    [Test]
    public async ValueTask IncludeFolderWithModulePath_HasConflictingHintNames() {
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

        await Assert.That(diagnostics).HasSingleItem();
    }
}
