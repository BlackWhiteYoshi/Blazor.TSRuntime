using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace TSRuntime.Tests;

public static class GeneratorSummaryTests {
    [Fact]
    public static Task SummaryOnly() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * The example Summary
             */
            export function test(): void;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        Assert.Equal(4, result.Length);
        string itsRuntimeModule = result[2];
        return Verify(itsRuntimeModule);
    }

    [Fact]
    public static Task RemarksOnly() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @remarks The example remark
             */
            export function test(): void;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        Assert.Equal(4, result.Length);
        string itsRuntimeModule = result[2];
        return Verify(itsRuntimeModule);
    }

    [Fact]
    public static Task ParamOnly() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @param a - a is not B
             */
            export function test(a: number): void;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        Assert.Equal(4, result.Length);
        string itsRuntimeModule = result[2];
        return Verify(itsRuntimeModule);
    }

    [Fact]
    public static Task ParamOnly_JSDoc() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @param {number} a - a is not B
             */
            export function test(a) { }

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.js", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        Assert.Equal(4, result.Length);
        string itsRuntimeModule = result[2];
        return Verify(itsRuntimeModule);
    }

    [Fact]
    public static Task ReturnsOnly() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @returns a string 
             */
            export function test(): string;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        Assert.Equal(4, result.Length);
        string itsRuntimeModule = result[2];
        return Verify(itsRuntimeModule);
    }

    [Fact]
    public static Task ReturnsOnly_JSDoc() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @returns {string} a string 
             */
            export function test() { }

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.js", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        Assert.Equal(4, result.Length);
        string itsRuntimeModule = result[2];
        return Verify(itsRuntimeModule);
    }


    [Fact]
    public static Task SummaryAndRemarksAndParamAndReturns() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * The example Summary
             *
             * @param a - a is not B
             *
             * @returns a string
             *
             * @remarks The example remark
             */
            export function test(a: number): string;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        Assert.Equal(4, result.Length);
        string itsRuntimeModule = result[2];
        return Verify(itsRuntimeModule);
    }

    [Fact]
    public static Task SummaryAndRemarksAndParamAndReturns_JSDocs() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * The example Summary
             *
             * @param {number} a - a is not B
             *
             * @returns {string} a string
             *
             * @remarks The example remark
             */
            export function test(a) { }

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.js", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        Assert.Empty(diagnostics);

        Assert.Equal(4, result.Length);
        string itsRuntimeModule = result[2];
        return Verify(itsRuntimeModule);
    }
}
