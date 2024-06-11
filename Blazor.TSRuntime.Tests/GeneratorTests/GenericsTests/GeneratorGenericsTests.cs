using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace TSRuntime.Tests;

public static class GeneratorGenericsTests {
    [Fact]
    public static Task JSGenerics() {
        const string jsonConfig = """{}""";
        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/GenericModule.d.ts", "export function generic<A, B, C>(): A;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        Assert.Equal(4, result.Length);
        return Verify($"""
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

    [Fact]
    public static Task JSGenericsConstraint() {
        const string jsonConfig = """{}""";
        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/GenericModule.d.ts", "export function genericKeyofConstraint<Type, Key extends keyof Type>(): void;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        Assert.Equal(4, result.Length);
        return Verify($"""
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

    [Fact]
    public static Task JSGenericsAndTypeMap() {
        const string jsonConfig = """{}""";
        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/GenericModule.d.ts", "export function genericKeyofConstraint<Type, Key extends keyof Type>(): number;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        Assert.Equal(4, result.Length);
        return Verify($"""
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
