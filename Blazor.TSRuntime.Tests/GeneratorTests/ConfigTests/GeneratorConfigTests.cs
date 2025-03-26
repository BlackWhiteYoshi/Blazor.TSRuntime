using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace TSRuntime.Tests;

public sealed class GeneratorConfigTests {
    private static readonly (string path, string content) testModule = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/TestModule.d.ts", "export function Test(a: number, b: string): number;\n");
    private static readonly (string path, string content) nestedTestModule = ($"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/NestedFolder/NestedTestModule.d.ts", "export declare function NestedTest(): Promise<void>;\n");


    [Test]
    public async ValueTask DefaultConfig() {
        const string jsonConfig = """{}""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask EverythingTurnedOff() {
        const string jsonConfig = """
            {
                "using statements": [],
                "invoke function": {
                  "sync enabled": false,
                  "trysync enabled": false,
                  "async enabled": false,
                  "promise": {
                    "only async enabled": false,
                    "append async": false
                  },
                  "type map": { }
                },
                "module grouping": {
                  "enabled": false,
                  "interface name pattern": {
                    "pattern": "I#module#Module",
                    "module transform": "first upper case"
                  }
                },
                "js runtime": {
                  "sync enabled": false,
                  "trysync enabled": false,
                  "async enabled": false
                },
                "service extension": false
            }
            """;
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}
            """);
    }


    [Test]
    public async ValueTask WebRootPath() {
        const string jsonConfig = """{ "webroot path": ".." }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    // InputPath is in InputPathTest.cs

    [Test]
    public async ValueTask UsingStatements() {
        const string jsonConfig = """{ "using statements": ["UsingStatement.1", "UsingStatement.2", "UsingStatement.3"] }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }


    [Test]
    public async ValueTask InvokeFunctionSyncEnabled() {
        const string jsonConfig = """{ "invoke function": { "sync enabled": true, "trysync enabled": false } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask InvokeFunctionTrySyncEnabled() {
        const string jsonConfig = """{ "invoke function": { "trysync enabled": true } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask InvokeFunctionAsyncEnabled() {
        const string jsonConfig = """{ "invoke function": { "trysync enabled": false, "async enabled": true } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }


    [Test]
    public async ValueTask InvokeFunctionNamePattern() {
        const string jsonConfig = """{ "invoke function": { "name pattern": { "pattern": "#module#_#function#_#action#" } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask InvokeFunctionModuleTransform() {
        const string jsonConfig = """{ "invoke function": { "name pattern": { "pattern": "#module#_#function#_#action#", "module transform": "lower case" } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask InvokeFunctionFunctionTransform() {
        const string jsonConfig = """{ "invoke function": { "name pattern": { "pattern": "#module#_#function#_#action#", "function transform": "lower case" } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask InvokeFunctionActionTransform() {
        const string jsonConfig = """{ "invoke function": { "name pattern": { "pattern": "#module#_#function#_#action#", "action transform": "lower case" } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask InvokeFunctionActionNameSync() {
        const string jsonConfig = """{ "invoke function": { "sync enabled": true, "name pattern": { "pattern": "#module#_#function#_#action#", "action name": { "sync": "TTT" } } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask InvokeFunctionActionNameTrySync() {
        const string jsonConfig = """{ "invoke function": { "name pattern": { "pattern": "#module#_#function#_#action#", "action name": { "trysync": "TTT" } } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask InvokeFunctionActionNameAsync() {
        const string jsonConfig = """{ "invoke function": { "name pattern": { "pattern": "#module#_#function#_#action#", "action name": { "async": "TTT" } } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }


    [Test]
    public async ValueTask PromiseOnlyAsync() {
        const string jsonConfig = """
            {
                "invoke function": {
                    "sync enabled": true,
                    "trysync enabled": true,
                    "async enabled": true,
                    "name pattern": {
                        "pattern": "#function##action#"
                    },
                    "promise": {
                        "only async enabled": true
                    }
                }
            }
            """;
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask PromiseAppendAsync() {
        const string jsonConfig = """{ "invoke function": { "promise": { "append async": true } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }


    #region TypeMap

    private const string MODULE_PATH = $"{GenerateSourceTextExtension.CONFIG_FOLDER_PATH}/module.d.ts";

    [Test]
    public async ValueTask TypeMap_MapsIdentity() {
        const string jsonConfig = """{ "invoke function": { "type map": { } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeModule = result[2];
        await Verify($"""
            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }


    [Test]
    public async ValueTask TypeMap_MapsNullable() {
        const string jsonConfig = """{}""";
        (string path, string content) module = (MODULE_PATH, "export function TT(a: number | null, b: string | null): number | null;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeModule = result[2];
        await Verify($"""
            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask TypeMap_MapsArray() {
        const string jsonConfig = """{}""";
        (string path, string content) module = (MODULE_PATH, "export function TT(a: number[], b: string[]): number[];\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeModule = result[2];
        await Verify($"""
            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask TypeMap_MapsNullableArray() {
        const string jsonConfig = """{}""";
        (string path, string content) module = (MODULE_PATH, "export function TT(a: number[] | null, b: string[] | null): number[] | null;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeModule = result[2];
        await Verify($"""
            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask TypeMap_MapsNullableArrayWithNullableItems() {
        const string jsonConfig = """{}""";
        (string path, string content) module = (MODULE_PATH, "export function TT(a: (number | null)[] | null, b: (string | null)[] | null): (number | null)[] | null;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeModule = result[2];
        await Verify($"""
            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }


    [Test]
    public async ValueTask TypeMap_MapsGeneric() {
        const string jsonConfig = """{ "invoke function": { "type map": { "number": { "type": "TN", "generic types": "TN" } } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeModule = result[2];
        await Verify($"""
            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask TypeMap_MapsMultipleGenerics() {
        const string jsonConfig = """{ "invoke function": { "type map": { "number": { "type": "Dictionary<K, V>", "generic types": ["K", "V"] } } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeModule = result[2];
        await Verify($"""
            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    [Test]
    public async ValueTask TypeMap_MapsGenericExceptOptionalIsNotIncluded() {
        const string jsonConfig = """{}""";
        (string path, string content) module = (MODULE_PATH, "export function Test(a: string | undefined, b: number | undefined): void;\n");
        string[] result = jsonConfig.GenerateSourceText([module], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string itsRuntimeModule = result[2];
        await Verify($"""
            ------
            Module
            ------

            {itsRuntimeModule}
            """);
    }

    #endregion


    [Test]
    public async ValueTask PreloadNamePattern() {
        const string jsonConfig = """{ "preload function": { "name pattern": { "pattern": "TTT_#module#" } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask PreloadModuleTransform() {
        const string jsonConfig = """{ "preload function": { "name pattern": { "module transform": "lower case" } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask PreloadAllModulesName() {
        const string jsonConfig = """{ "preload function": { "all modules name": "TTT" } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }


    [Test]
    public async ValueTask ModuleGrouping() {
        const string jsonConfig = """{ "module grouping": { "enabled": true } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask ModuleGroupingNamePattern() {
        const string jsonConfig = """{ "module grouping": { "enabled": true, "interface name pattern": { "pattern": "TTT_#module#" } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask ModuleGroupingModuleTransform() {
        const string jsonConfig = """{ "module grouping": { "enabled": true, "interface name pattern": { "module transform": "lower case" } } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }


    [Test]
    public async ValueTask JSRuntimeSyncEnabled() {
        const string jsonConfig = """{ "js runtime": { "sync enabled": true } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask JSRuntimeTrySyncEnabled() {
        const string jsonConfig = """{ "js runtime": { "trysync enabled": true } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }

    [Test]
    public async ValueTask JSRuntimeAsyncEnabled() {
        const string jsonConfig = """{ "js runtime": { "async enabled": true } }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }


    [Test]
    public async ValueTask ServiceExtension() {
        const string jsonConfig = """{ "service extension": true }""";
        string[] result = jsonConfig.GenerateSourceText([testModule, nestedTestModule], out _, out ImmutableArray<Diagnostic> diagnostics);
        await Assert.That(diagnostics).IsEmpty();

        string tsRuntime = result[0];
        string itsRuntimeCore = result[1];
        string itsRuntimeModule = result[2];
        string itsRuntimeNestedModule = result[3];
        string serviceExtension = result[4];
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

            -------------
            Nested Module
            -------------

            {itsRuntimeNestedModule}

            ----------------
            ServiceExtension
            ----------------

            {serviceExtension}
            """);
    }
}
