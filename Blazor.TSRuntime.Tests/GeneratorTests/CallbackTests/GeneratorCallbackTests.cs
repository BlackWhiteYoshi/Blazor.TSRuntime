using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace TSRuntime.Tests;

public sealed class GeneratorCallbackTests {
    private const string SCRIPT_PATH = $"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/site.d.ts";

    [Test]
    public async ValueTask Parameterless() {
        const string jsonConfig = """{}""";
        const string content = "export function callbackTest(someCallback: () => void): void;\n";

        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, content)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime.XVersionNumber()}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore.XVersionNumber()}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask ParameterAndReturnType() {
        const string jsonConfig = """{}""";
        const string content = "export function callbackTest(parseString: (str: string) => number): number;\n";

        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, content)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime.XVersionNumber()}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore.XVersionNumber()}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask MultipleParameter() {
        const string jsonConfig = """{}""";
        const string content = "export function callbackTest(a: boolean, parseString: (str: string) => number, b: number, callback2: (n: number) => string): void;\n";

        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, content)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime.XVersionNumber()}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore.XVersionNumber()}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask Promise() {
        const string jsonConfig = """{}""";
        const string content = "export function callbackTest(someCallback: () => Promise<string>): void;\n";

        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, content)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime.XVersionNumber()}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore.XVersionNumber()}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask PromiseVoid() {
        const string jsonConfig = """{}""";
        const string content = "export function callbackTest(someCallback: () => Promise<void>): void;\n";

        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, content)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime.XVersionNumber()}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore.XVersionNumber()}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask Script() {
        const string jsonConfig = """
            {
                "input path": {
                    "include": "/",
                    "module files": false
                }
            }
            """;
        const string content = "function callbackTest(someCallback: () => void): void;\n";

        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, content)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime.XVersionNumber()}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore.XVersionNumber()}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask ReturnTypeNotSupported() {
        const string jsonConfig = """{}""";
        const string content = "export function callbackTest(): () => void;\n";

        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, content)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime.XVersionNumber()}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore.XVersionNumber()}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask NestedNotSupported() {
        const string jsonConfig = """{}""";
        const string content = "export function callbackTest(someCallback: (nestedCallback: () => void) => void): void;\n";

        string[] result = jsonConfig.GenerateSourceText([(SCRIPT_PATH, content)], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime.XVersionNumber()}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore.XVersionNumber()}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }
}
