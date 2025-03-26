using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace TSRuntime.Tests;

public sealed class ScriptTests {
    private const string SCRIPT_PATH = $"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/site.ts";

    [Test]
    public async ValueTask ParameterlessFunction() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "/",
                    "module files": false
                },
                "invoke function": {
                    "sync enabled": true,
                    "trysync enabled": true,
                    "async enabled": true,
                    "name pattern": {
                        "pattern": "#function##action#",
                        "module transform": "first upper case",
                        "function transform": "first upper case",
                        "action transform": "none"
                    }
                }
            }
            """;
        const string scriptFunction = "function Test() {}\n";
        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, scriptFunction)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeScript = result[2];
        await Verify(itsRuntimeScript);
    }

    [Test]
    public async ValueTask ParameterAndReturnTypeFunction() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "/",
                    "module files": false
                },
                "invoke function": {
                    "sync enabled": true,
                    "trysync enabled": true,
                    "async enabled": true,
                    "name pattern": {
                        "pattern": "#function##action#",
                        "module transform": "first upper case",
                        "function transform": "first upper case",
                        "action transform": "none"
                    }
                }
            }
            """;
        const string scriptFunction = "function Test(str: string, a: boolean): number {}\n";
        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, scriptFunction)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeScript = result[2];
        await Verify(itsRuntimeScript);
    }

    [Test]
    public async ValueTask PromiseFunction() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "/",
                    "module files": false
                },
                "invoke function": {
                    "sync enabled": true,
                    "trysync enabled": true,
                    "async enabled": true,
                    "name pattern": {
                        "pattern": "#function##action#",
                        "module transform": "first upper case",
                        "function transform": "first upper case",
                        "action transform": "none"
                    }
                }
            }
            """;
        const string scriptFunction = "function Test(): Promise<void> {}\n";
        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, scriptFunction)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeScript = result[2];
        await Verify(itsRuntimeScript);
    }

    [Test]
    public async ValueTask PromiseReturnFunction() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "/",
                    "module files": false
                },
                "invoke function": {
                    "sync enabled": true,
                    "trysync enabled": true,
                    "async enabled": true,
                    "name pattern": {
                        "pattern": "#function##action#",
                        "module transform": "first upper case",
                        "function transform": "first upper case",
                        "action transform": "none"
                    }
                }
            }
            """;
        const string scriptFunction = "function Test(): Promise<number> {}\n";
        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, scriptFunction)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeScript = result[2];
        await Verify(itsRuntimeScript);
    }
}
