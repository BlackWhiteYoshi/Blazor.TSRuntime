using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace TSRuntime.Tests;

public sealed class GeneratorSummaryTests {
    [Test]
    public async ValueTask SummaryOnly() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * The example Summary
             */
            export function test(): void;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        await Assert.That(result.Length).IsEqualTo(4);
        string itsRuntimeModule = result[2];
        await Verify(itsRuntimeModule);
    }

    [Test]
    public async ValueTask RemarksOnly() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @remarks The example remark
             */
            export function test(): void;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        await Assert.That(result.Length).IsEqualTo(4);
        string itsRuntimeModule = result[2];
        await Verify(itsRuntimeModule);
    }

    [Test]
    public async ValueTask ParamOnly() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @param a - a is not B
             */
            export function test(a: number): void;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        await Assert.That(result.Length).IsEqualTo(4);
        string itsRuntimeModule = result[2];
        await Verify(itsRuntimeModule);
    }

    [Test]
    public async ValueTask ParamOnly_JSDoc() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @param {number} a - a is not B
             */
            export function test(a) { }

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.js", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        await Assert.That(result.Length).IsEqualTo(4);
        string itsRuntimeModule = result[2];
        await Verify(itsRuntimeModule);
    }

    [Test]
    public async ValueTask ReturnsOnly() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @returns a string 
             */
            export function test(): string;

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        await Assert.That(result.Length).IsEqualTo(4);
        string itsRuntimeModule = result[2];
        await Verify(itsRuntimeModule);
    }

    [Test]
    public async ValueTask ReturnsOnly_JSDoc() {
        const string jsonConfig = """{}""";
        const string moduleContent = """
            /**
             * @returns {string} a string 
             */
            export function test() { }

            """;

        (string path, string content) module = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.js", moduleContent);
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        await Assert.That(result.Length).IsEqualTo(4);
        string itsRuntimeModule = result[2];
        await Verify(itsRuntimeModule);
    }


    [Test]
    public async ValueTask SummaryAndRemarksAndParamAndReturns() {
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
        await Assert.That(diagnostics).IsEmpty();

        await Assert.That(result.Length).IsEqualTo(4);
        string itsRuntimeModule = result[2];
        await Verify(itsRuntimeModule);
    }

    [Test]
    public async ValueTask SummaryAndRemarksAndParamAndReturns_JSDocs() {
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
        await Assert.That(diagnostics).IsEmpty();

        await Assert.That(result.Length).IsEqualTo(4);
        string itsRuntimeModule = result[2];
        await Verify(itsRuntimeModule);
    }
}
