using System.Text;
using TSRuntime.Core.Configs;
using TSRuntime.Core.Configs.NamePattern;
using TSRuntime.Core.Generation;
using TSRuntime.Core.Parsing;
using Xunit;

namespace TSRuntime.Core.Tests;

public sealed class CoreGeneratorTest {
    private static string GetITSRuntimeContent(IEnumerable<string> content) {
        StringBuilder builder = new(10000);

        foreach (string str in content)
            builder.Append(str);

        return builder.ToString();
    }

    private static TSStructureTree CreateExampleStructureTree() {
        return new TSStructureTree() {
            ModuleList = new List<TSModule>() {
                new TSModule() {
                    ModuleName = "Test",
                    FilePath = "/test",
                    ModulePath = "/test.js",
                    FunctionList = new List<TSFunction>() {
                        new TSFunction() {
                            Name = "Test",
                            ParameterList = new List<TSParameter>() {
                                new TSParameter() {
                                    Name = "a",
                                    Type = "number"
                                },
                                new TSParameter() {
                                    Name = "b",
                                    Type = "string"
                                }
                            },
                            ReturnType = new TSParameter() {
                                Name = "ReturnType",
                                Type = "number"
                            }
                        }
                    }
                }
            }
        };
    }

    
    [Fact]
    public void PreloadMethodExists() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new();

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public Task PreloadTest()
                    => GetOrLoadModule(0, "/test.js");
            """;
        Assert.Contains(expected, content);
    }


    #region Invoke methods

    private const string INVOKE = """
            public double Test(double a, string b)
                => Invoke<double>(0, "/test.js", "Test", a, b);
        """;

    private const string INVOKE_TRYSYNC = """
            public ValueTask<double> Test(double a, string b, CancellationToken cancellationToken = default)
                => InvokeTrySync<double>(0, "/test.js", "Test", cancellationToken, a, b);
        """;

    private const string INVOKE_ASYNC = """
            public ValueTask<double> Test(double a, string b, CancellationToken cancellationToken = default)
                => InvokeAsync<double>(0, "/test.js", "Test", cancellationToken, a, b);
        """;

    [Fact]
    public void InvokeMethodExists_WhenModuleInvokeEnabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionSyncEnabled = true,
            InvokeFunctionTrySyncEnabled = false,
            InvokeFunctionAsyncEnabled = false
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.Contains(INVOKE, content);
        Assert.DoesNotContain(INVOKE_TRYSYNC, content);
        Assert.DoesNotContain(INVOKE_ASYNC, content);
    }

    [Fact]
    public void InvokeTrySyncMethodExists_WhenModuleInvokeTrySyncEnabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionSyncEnabled = false,
            InvokeFunctionTrySyncEnabled = true,
            InvokeFunctionAsyncEnabled = false
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.DoesNotContain(INVOKE, content);
        Assert.Contains(INVOKE_TRYSYNC, content);
        Assert.DoesNotContain(INVOKE_ASYNC, content);
    }

    [Fact]
    public void InvokeAsyncMethodExists_WhenModuleInvokeAsyncEnabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionSyncEnabled = false,
            InvokeFunctionTrySyncEnabled = false,
            InvokeFunctionAsyncEnabled = true
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.DoesNotContain(INVOKE, content);
        Assert.DoesNotContain(INVOKE_TRYSYNC, content);
        Assert.Contains(INVOKE_ASYNC, content);
    }

    [Fact]
    public void NoInvokeMethodExists_WhenAllDisabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionSyncEnabled = false,
            InvokeFunctionTrySyncEnabled = false,
            InvokeFunctionAsyncEnabled = false
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.DoesNotContain(INVOKE, content);
        Assert.DoesNotContain(INVOKE_TRYSYNC, content);
        Assert.DoesNotContain(INVOKE_ASYNC, content);
    }

    [Fact]
    public void AllInvokeMethodExists_WhenModuleAllEnabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionSyncEnabled = true,
            InvokeFunctionTrySyncEnabled = true,
            InvokeFunctionAsyncEnabled = true
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.Contains(INVOKE, content);
        Assert.Contains(INVOKE_TRYSYNC, content);
        Assert.Contains(INVOKE_ASYNC, content);
    }

    #endregion


    #region JSRuntime invoke

    private const string JS_INVOKE = """
            public TResult Invoke<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(string identifier, params object?[]? args)
                => ((IJSInProcessRuntime)JsRuntime).Invoke<TResult>(identifier, args);
        """;

    private const string JS_INVOKE_TRYSYNC = """
            public ValueTask<TValue> InvokeTrySync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, params object?[]? args) {
                if (JsRuntime is IJSInProcessRuntime jsInProcessRuntime)
                    return ValueTask.FromResult(jsInProcessRuntime.Invoke<TValue>(identifier, args));
                else
                    return JsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);
            }
        """;

    private const string JS_INVOKE_ASYNC = """
            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, params object?[]? args)
                => JsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);
        """;

    [Fact]
    public void InvokeMethodExists_WhenJSRuntimeInvokeEnabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            JSRuntimeSyncEnabled = true,
            JSRuntimeTrySyncEnabled = false,
            JSRuntimeAsyncEnabled = false
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.Contains(JS_INVOKE, content);
        Assert.DoesNotContain(JS_INVOKE_TRYSYNC, content);
        Assert.DoesNotContain(JS_INVOKE_ASYNC, content);
    }

    [Fact]
    public void InvokeTrySyncMethodExists_WhenJSRuntimeInvokeTrySyncEnabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            JSRuntimeSyncEnabled = false,
            JSRuntimeTrySyncEnabled = true,
            JSRuntimeAsyncEnabled = false
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.DoesNotContain(JS_INVOKE, content);
        Assert.Contains(JS_INVOKE_TRYSYNC, content);
        Assert.DoesNotContain(JS_INVOKE_ASYNC, content);
    }

    [Fact]
    public void InvokeAsyncMethodExists_WhenJSRuntimeInvokeAsyncEnabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            JSRuntimeSyncEnabled = false,
            JSRuntimeTrySyncEnabled = false,
            JSRuntimeAsyncEnabled = true
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.DoesNotContain(JS_INVOKE, content);
        Assert.DoesNotContain(JS_INVOKE_TRYSYNC, content);
        Assert.Contains(JS_INVOKE_ASYNC, content);
    }

    [Fact]
    public void NoInvokeMethodExists_WhenJSRuntimeAllDisabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            JSRuntimeSyncEnabled = false,
            JSRuntimeTrySyncEnabled = false,
            JSRuntimeAsyncEnabled = false
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.DoesNotContain(JS_INVOKE, content);
        Assert.DoesNotContain(JS_INVOKE_TRYSYNC, content);
        Assert.DoesNotContain(JS_INVOKE_ASYNC, content);
    }

    [Fact]
    public void AllInvokeMethodExists_WhenJSRuntimeAllEnabled() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            JSRuntimeSyncEnabled = true,
            JSRuntimeTrySyncEnabled = true,
            JSRuntimeAsyncEnabled = true
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.Contains(JS_INVOKE, content);
        Assert.Contains(JS_INVOKE_TRYSYNC, content);
        Assert.Contains(JS_INVOKE_ASYNC, content);
    }

    #endregion


    #region Promise Function

    [Fact]
    public void PromiseFunctionOnlyAsync() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        structureTree.ModuleList[0].FunctionList[0].ReturnPromise = true;
        Config config = new() {
            InvokeFunctionSyncEnabled = true,
            InvokeFunctionTrySyncEnabled = true,
            InvokeFunctionAsyncEnabled = true,
            PromiseOnlyAsync = true
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.DoesNotContain(INVOKE, content);
        Assert.DoesNotContain(INVOKE_TRYSYNC, content);
        Assert.Contains(INVOKE_ASYNC, content);
    }

    [Fact]
    public void PromiseFunctionAppendAsync() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        structureTree.ModuleList[0].FunctionList[0].ReturnPromise = true;
        Config config = new() {
            InvokeFunctionSyncEnabled = true,
            InvokeFunctionTrySyncEnabled = true,
            InvokeFunctionAsyncEnabled = true,
            PromiseOnlyAsync = true,
            PromiseAppendAsync = true
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public ValueTask<double> TestAsync(double a, string b, CancellationToken cancellationToken = default)
                    => InvokeAsync<double>(0, "/test.js", "Test", cancellationToken, a, b);
            """;
        Assert.Contains(expected, content);
    }

    #endregion


    #region name pattern

    [Theory]
    [InlineData("MyMehtod")]
    [InlineData("banana")]
    [InlineData("")]
    [InlineData("asdf")]
    public void FunctionNmaePattern_Constant(string name) {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            InvokeFunctionNamePattern = new(name, NameTransform.None, NameTransform.None, NameTransform.None)
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        string expected = $"""
            public ValueTask<double> {name}(double a, string b, CancellationToken cancellationToken = default)
                => InvokeTrySync<double>(0, "/test.js", "Test", cancellationToken, a, b);
        """;
        Assert.Contains(expected, content);
    }

    [Fact]
    public void FunctionNmaePattern_Variable() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            InvokeFunctionNamePattern = new("My#module##function##action#", NameTransform.None, NameTransform.None, NameTransform.None)
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
            public ValueTask<double> MyTestTestInvokeTrySync(double a, string b, CancellationToken cancellationToken = default)
                => InvokeTrySync<double>(0, "/test.js", "Test", cancellationToken, a, b);
        """;
        Assert.Contains(expected, content);
    }

    [Fact]
    public void FunctionNmaePattern_NameTransform() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            InvokeFunctionNamePattern = new("My#module##function##action#", NameTransform.UpperCase, NameTransform.UpperCase, NameTransform.UpperCase)
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
            public ValueTask<double> MyTESTTESTINVOKETRYSYNC(double a, string b, CancellationToken cancellationToken = default)
                => InvokeTrySync<double>(0, "/test.js", "Test", cancellationToken, a, b);
        """;
        Assert.Contains(expected, content);
    }

    [Theory]
    [InlineData("MyMehtod")]
    [InlineData("banana")]
    [InlineData("")]
    [InlineData("asdf")]
    public void PreloadNmaePattern_Constant(string name) {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            PreloadNamePattern = new(name, NameTransform.None)
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        string expected = $"""
                public Task {name}()
                    => GetOrLoadModule(0, "/test.js");
            """;
        Assert.Contains(expected, content);
    }

    [Fact]
    public void PreloadNmaePattern_Variable() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            PreloadNamePattern = new("My#module#", NameTransform.None)
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = $"""
                public Task MyTest()
                    => GetOrLoadModule(0, "/test.js");
            """;
        Assert.Contains(expected, content);
    }

    [Fact]
    public void PreloadNmaePattern_NameTransform() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            PreloadNamePattern = new("My#module#", NameTransform.UpperCase)
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = $"""
                public Task MyTEST()
                    => GetOrLoadModule(0, "/test.js");
            """;
        Assert.Contains(expected, content);
    }

    #endregion


    [Fact]
    public void PreloadAllModulesName() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            PreloadAllModulesName = "PreloadEverything"
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public Task PreloadEverything() {
                    PreloadTest();

                    return Task.WhenAll(Modules!);
                }
            """;
        Assert.Contains(expected, content);
    }


    [Fact]
    public void UsingStatements() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            UsingStatements = new string[] { "System", "banana", "", "asdf" }
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        Assert.Contains("using System;", content);
        Assert.Contains("using banana;", content);
        Assert.Contains("using ;", content);
        Assert.Contains("using asdf;", content);
        Assert.DoesNotContain("using Microsoft.AspNetCore.Components;", content);
    }


    #region type map

    [Fact]
    public void TypeMap_MapsIdentity_WhenEmpty() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            TypeMap = new()
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public ValueTask<number> Test(number a, string b, CancellationToken cancellationToken = default)
                    => InvokeTrySync<number>(0, "/test.js", "Test", cancellationToken, a, b);
            """;

        Assert.Contains(expected, content);
    }

    [Fact]
    public void TypeMap_MapsIdentityNullable_WhenEmptyAndNullable() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        structureTree.ModuleList[0].FunctionList[0].ReturnType.TypeNullable = true;
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            TypeMap = new()
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public ValueTask<number?> Test(number a, string b, CancellationToken cancellationToken = default)
                    => InvokeTrySync<number?>(0, "/test.js", "Test", cancellationToken, a, b);
            """;

        Assert.Contains(expected, content);
    }

    [Fact]
    public void TypeMap_MapsIdentityArray_WhenEmptyAndArray() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        structureTree.ModuleList[0].FunctionList[0].ReturnType.Array = true;
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            TypeMap = new()
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public ValueTask<number[]> Test(number a, string b, CancellationToken cancellationToken = default)
                    => InvokeTrySync<number[]>(0, "/test.js", "Test", cancellationToken, a, b);
            """;

        Assert.Contains(expected, content);
    }

    [Fact]
    public void TypeMap_MapsIdentityArrayNullable_WhenEmptyAndArrayNullable() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        structureTree.ModuleList[0].FunctionList[0].ReturnType.Array = true;
        structureTree.ModuleList[0].FunctionList[0].ReturnType.ArrayNullable = true;
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            TypeMap = new()
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public ValueTask<number[]?> Test(number a, string b, CancellationToken cancellationToken = default)
                    => InvokeTrySync<number[]?>(0, "/test.js", "Test", cancellationToken, a, b);
            """;

        Assert.Contains(expected, content);
    }

    [Fact]
    public void TypeMap_MapsIdentityNullableArrayNullable_WhenEmptyAndNullableArrayNullable() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        structureTree.ModuleList[0].FunctionList[0].ReturnType.TypeNullable = true;
        structureTree.ModuleList[0].FunctionList[0].ReturnType.Array = true;
        structureTree.ModuleList[0].FunctionList[0].ReturnType.ArrayNullable = true;
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            TypeMap = new()
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public ValueTask<number?[]?> Test(number a, string b, CancellationToken cancellationToken = default)
                    => InvokeTrySync<number?[]?>(0, "/test.js", "Test", cancellationToken, a, b);
            """;

        Assert.Contains(expected, content);
    }

    [Fact]
    public void TypeMap_MapsToCorrespondingString() {
        TSStructureTree structureTree = CreateExampleStructureTree();
        Config config = new() {
            InvokeFunctionTrySyncEnabled = true,
            TypeMap = new() {
                ["number"] = "A",
                ["string"] = "B"
            }
        };

        IEnumerable<string> tsRuntimeContent = Generator.GetITSRuntimeContent(structureTree, config);
        string content = GetITSRuntimeContent(tsRuntimeContent);

        const string expected = """
                public ValueTask<A> Test(A a, B b, CancellationToken cancellationToken = default)
                    => InvokeTrySync<A>(0, "/test.js", "Test", cancellationToken, a, b);
            """;

        Assert.Contains(expected, content);
    }

    #endregion
}
