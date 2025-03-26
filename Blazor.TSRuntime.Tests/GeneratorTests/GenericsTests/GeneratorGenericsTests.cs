using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace TSRuntime.Tests;

public sealed class GeneratorGenericsTests {
    [Test]
    public async ValueTask JSGenerics() {
        const string jsonConfig = """{}""";
        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/GenericModule.d.ts", "export function generic<A, B, C>(): A;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Assert.That(result.Length).IsEqualTo(4);
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask JSGenericsConstraint() {
        const string jsonConfig = """{}""";
        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/GenericModule.d.ts", "export function genericKeyofConstraint<Type, Key extends keyof Type>(): void;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Assert.That(result.Length).IsEqualTo(4);
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask JSGenericsAndTypeMap() {
        const string jsonConfig = """{}""";
        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/GenericModule.d.ts", "export function genericKeyofConstraint<Type, Key extends keyof Type>(): number;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        await Assert.That(result.Length).IsEqualTo(4);
        await Verify($"""
            ---------
            TSRuntime
            ---------

            {tsRuntime}

            ----------
            ITSRuntime
            ----------

            {itsRuntimeCore}

            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }
}
