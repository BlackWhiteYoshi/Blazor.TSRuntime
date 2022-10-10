using Microsoft.CodeAnalysis;

namespace TSRuntime.Generation;

[Generator]
public sealed class SourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput((IncrementalGeneratorPostInitializationContext context) => {
            Parser parser = new();

            parser.Parse(DECLARATIONS.AsSpan());
            parser.Parse(PRE_LOAD.AsSpan());
            parser.Parse(CreateModule().AsSpan());
            parser.Parse(PRIVATE_INVOKE_METHODS.AsSpan());
            parser.Parse(JSRuntime_METHODS.AsSpan());

            string source = $$"""
                // --- <auto generated> ---
        
        
                using TSRuntime.Core.Configs;
                using TSRuntime.Core.Parsing;
        
                namespace TSRuntime.Core.Generation;
        
                public static partial class Generator {
                    public static partial IEnumerable<string> GetITSRuntimeContent(TSSyntaxTree syntaxTree, Config config) {
                {{parser.GetContent()}}    }
                }

                """;

            context.AddSource("Generator.g.cs", source);
        });
    }


    #region Content

    /// <summary>
    /// Includes using statements, namespace and interface name.
    /// </summary>
    private const string DECLARATIONS = """
        // --- <auto generated> ---
        
        
        ``
        foreach (string usingStatement in config.UsingStatements) {
        `+
        using `usingStatement`;
        ``
        }
        `-
        using Microsoft.JSInterop.Infrastructure;
        using System.Diagnostics.CodeAnalysis;
        using System.Threading;
        using System.Threading.Tasks;
        
        namespace Microsoft.JSInterop;
        
        public interface ITSRuntime {
        
        """;

    /// <summary>
    /// Includes the fields MODULE_COUNT and JsRuntime.<br />
    /// Includes the PreLoad_"moduleName" functions and the GetOrLoadModule declaration.
    /// </summary>
    private const string PRE_LOAD = """
            protected const int MODULE_COUNT = `syntaxTree.ModuleList.Count.ToString()`;
        
            protected IJSRuntime JsRuntime { get; }
        
        
        ``
        for (int i = 0; i < syntaxTree.ModuleList.Count; i++) {
            TSModule module = syntaxTree.ModuleList[i];
            string index = i.ToString();
        `+
            /// <summary>
            /// <para>Preloads `module.ModuleName` (`module.ModulePath`) as javascript-module.</para>
            /// <para>If already loading, it doesn't trigger a second loading and if already loaded, it returns a completed task.</para>
            /// </summary>
            public async ValueTask PreLoad_`module.ModuleName`()
                => await GetOrLoadModule(`index`, "`module.ModulePath`");
        
        ``
        }
        `-
        
            protected Task<IJSObjectReference> GetOrLoadModule(int index, string url);

        """;

    /// <summary>
    /// Includes all the module functions Invoke, InvokeTrySync and InvokeAsync.
    /// </summary>
    private static string CreateModule() {
        return $$"""
            ``
            for (int i = 0; i < syntaxTree.ModuleList.Count; i++) {
                TSModule module = syntaxTree.ModuleList[i];
                string index = i.ToString();
            `+
        
        
                #region `module.ModuleName`
            ``
            foreach (TSFunction function in module.FunctionList) {
                string returnTypeMapped = config.TypeMap.ValueOrKey(function.ReturnType.Type);
                (List<string> parameters, List<string> arguments) = ParamterArgumentList(function, config.TypeMap);
            `+
            ``
            if (!function.ReturnPromise) {
            `+
            ``
            if (config.ModuleInvokeEnabled) {
            `+

            {{GetInvokeMethod(outParameter: false)}}

            {{GetInvokeMethod(outParameter: true)}}
            ``
            }
            `-
            ``
            if (config.ModuleTrySyncEnabled) {
            `+

            {{Get_TrySync_Async(trySync: true)}}
            ``
            }
            `-
            ``
            if (config.ModuleAsyncEnabled) {
            `+

            {{Get_TrySync_Async(trySync: false)}}
            ``
            }
            `-
            
            ``
            }
            `-
            ``
            else {
            `+

            {{Get_TrySync_Async(trySync: false)}}
            ``
            }
            `-
            ``
            }
            `-
                #endregion
            ``
            }
            `-
            """;


        static string GetInvokeMethod(bool outParameter) {
            string summaryOut;
            string parameters;
            string argumentOut;
            if (outParameter) {
                summaryOut = """
            
                    /// <param name="success">false when the module is not loaded, otherwise true</param>
                """;
                parameters = $"{PARAMETERS}out bool success";
                argumentOut = "success";
            }
            else {
                summaryOut = string.Empty;
                parameters = """
                    ``
                    for (int __i = 0; __i < parameters.Count - 1; __i++)
                        yield return parameters[__i];
                    ``
                    """;
                argumentOut = "_";
            }

            return $"""
                    /// <summary>
                    /// <para>Invokes in module `module.ModuleName` the js-function `function.Name` synchronously.</para>
                    /// <para>If module is not loaded, it returns without any invoking. If synchronous is not supported, it fails with an exception.</para>
                    /// </summary>
                {SUMMARY_PARAMETERS}{summaryOut}
                    /// <returns>default when the module is not loaded, otherwise result of the js-function</returns>
                    public `returnTypeMapped` {GetFunctionNamePattern("Invoke")}({parameters})
                        => Invoke<{MAPPED_IJS_VOID_RESULT}>(`index`, "`module.ModulePath`", "`function.Name`", out {argumentOut}{ARGUMENTS});
                """;
        }

        static string Get_TrySync_Async(bool trySync) {
            string summaryDescription;
            string methodName;
            if (trySync) {
                summaryDescription = "synchronously when supported, otherwise asynchronously";
                methodName = "TrySync";
            }
            else {
                summaryDescription = "asynchronously";
                methodName = "Async";
            }

            return $"""
                    /// <summary>
                    /// Invokes in module `module.ModuleName` the js-function `function.Name` {summaryDescription}.
                    /// </summary>
                {SUMMARY_PARAMETERS}
                    /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                    /// <returns></returns>
                    public {MAPPED_ASYNC} {GetFunctionNamePattern(methodName)}({PARAMETERS}CancellationToken cancellationToken = default)
                        =>{MAPPED_AWAIT} Invoke{methodName}<{MAPPED_IJS_VOID_RESULT}>(`index`, "`module.ModulePath`", "`function.Name`", cancellationToken{ARGUMENTS});
                """;
        }



        static string GetFunctionNamePattern(string action) {
            return $"""
                ``
                foreach (string str in config.FunctionNamePattern.GetNaming(function.Name, module.ModuleName, "{action}"))
                    yield return str;
                ``
                """;
        } 
    }

    /// <summary>
    /// Includes the private methods Invoke, InvokeTrySync, InvokeAsync
    /// </summary>
    private const string PRIVATE_INVOKE_METHODS = """
        

            /// <summary>
            /// <para>Invokes the specified JavaScript function in the specified module synchronously.</para>
            /// <para>If module is not loaded, it returns without any invoking. If synchronous is not supported, it fails with an exception.</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="moduleUrl">complete path of the module, e.g. "/Pages/Example.razor.js"</param>
            /// <param name="identifier">name of the javascript function</param>
            /// <param name="success">false when the module is not loaded, otherwise true</param>
            /// <param name="args">parameter passing to the js-function</param>
            /// <returns>default when the module is not loaded, otherwise result of the js-function</returns>
            private TResult Invoke<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(int moduleIndex, string moduleUrl, string identifier, out bool success, params object?[]? args) {
                Task<IJSObjectReference> moduleTask = GetOrLoadModule(moduleIndex, moduleUrl);
                if (!moduleTask.IsCompletedSuccessfully) {
                    success = false;
                    return default!;
                }
        
                success = true;
                return ((IJSInProcessObjectReference)moduleTask.Result).Invoke<TResult>(identifier, args);
            }
        
            /// <summary>
            /// Invokes the specified JavaScript function in the specified module synchronously when supported, otherwise asynchronously.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="moduleUrl">complete path of the module, e.g. "/Pages/Example.razor.js"</param>
            /// <param name="identifier">name of the javascript function</param>
            /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
            /// <param name="args">parameter passing to the js-function</param>
            /// <returns></returns>
            private async ValueTask<TValue> InvokeTrySync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(int moduleIndex, string moduleUrl, string identifier, CancellationToken cancellationToken, params object?[]? args) {
                IJSObjectReference module = await GetOrLoadModule(moduleIndex, moduleUrl);
                if (module is IJSInProcessObjectReference inProcessModule)
                    return inProcessModule.Invoke<TValue>(identifier, args);
                else
                    return await module.InvokeAsync<TValue>(identifier, cancellationToken, args);
            }
        
            /// <summary>
            /// Invokes the specified JavaScript function in the specified module asynchronously.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="moduleUrl">complete path of the module, e.g. "/Pages/Example.razor.js"</param>
            /// <param name="identifier">name of the javascript function</param>
            /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
            /// <param name="args">parameter passing to the js-function</param>
            /// <returns></returns>
            private async ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(int moduleIndex, string moduleUrl, string identifier, CancellationToken cancellationToken, params object?[]? args) {
                IJSObjectReference module = await GetOrLoadModule(moduleIndex, moduleUrl);
                return await module.InvokeAsync<TValue>(identifier, cancellationToken, args);
            }

        """;

    /// <summary>
    /// Includes non typed methods
    /// </summary>
    private const string JSRuntime_METHODS = """
        
        
            #region JSRuntime methods
        ``
        if (config.JSRuntimeInvokeEnabled) {
        `+

            /// <summary>
            /// Invokes the specified JavaScript function synchronously.
            /// </summary>
            /// <param name="jsRuntime">The <see cref="IJSInProcessRuntime"/>.</param>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="args">JSON-serializable arguments.</param>
            public void InvokeVoid(string identifier, params object?[]? args)
                => Invoke<IJSVoidResult>(identifier, args);
        
            /// <summary>
            /// Invokes the specified JavaScript function synchronously.
            /// </summary>
            /// <typeparam name="TResult">The JSON-serializable return type.</typeparam>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns>An instance of <typeparamref name="TResult"/> obtained by JSON-deserializing the return value.</returns>
            public TResult Invoke<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(string identifier, params object?[]? args)
                => ((IJSInProcessRuntime)JsRuntime).Invoke<TResult>(identifier, args);

        ``
        }
        `-
        ``
        if (config.JSRuntimeTrySyncEnabled) {
        `+

            /// <summary>
            /// This method performs synchronous, if the underlying implementation supports synchrounous interoperability.
            /// </summary>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns></returns>
            public async ValueTask InvokeVoidTrySync(string identifier, params object?[]? args)
                => await InvokeTrySync<IJSVoidResult>(identifier, default, args);
        
            /// <summary>
            /// This method performs synchronous, if the underlying implementation supports synchrounous interoperability.
            /// </summary>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns></returns>
            public async ValueTask InvokeVoidTrySync(string identifier, CancellationToken cancellationToken, params object?[]? args)
                => await InvokeTrySync<IJSVoidResult>(identifier, cancellationToken, args);
        
            /// <summary>
            /// This method performs synchronous, if the underlying implementation supports synchrounous interoperability.
            /// </summary>
            /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
            public ValueTask<TValue> InvokeTrySync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, params object?[]? args)
                => InvokeTrySync<TValue>(identifier, default, args);
        
            /// <summary>
            /// This method performs synchronous, if the underlying implementation supports synchrounous interoperability.
            /// </summary>
            /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
            public ValueTask<TValue> InvokeTrySync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, params object?[]? args) {
                if (JsRuntime is IJSInProcessRuntime jsInProcessRuntime)
                    return ValueTask.FromResult(jsInProcessRuntime.Invoke<TValue>(identifier, args));
                else
                    return JsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);
            }
        
        ``
        }
        `-
        ``
        if (config.JSRuntimeAsyncEnabled) {
        `+

            /// <summary>
            /// Invokes the specified JavaScript function asynchronously.
            /// <para>
            /// <see cref="JSRuntime"/> will apply timeouts to this operation based on the value configured in <see cref="JSRuntime.DefaultAsyncTimeout"/>. To dispatch a call with a different timeout, or no timeout,
            /// consider using <see cref="InvokeVoidAsync{TValue}(string, CancellationToken, object[])" />.
            /// </para>
            /// </summary>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns></returns>
            public async ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
                => await InvokeAsync<IJSVoidResult>(identifier, default, args);
        
            /// <summary>
            /// Invokes the specified JavaScript function asynchronously.
            /// </summary>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="cancellationToken">
            /// A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts
            /// (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.
            /// </param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns></returns>
            public async ValueTask InvokeVoidAsync(string identifier, CancellationToken cancellationToken, params object?[]? args)
                => await InvokeAsync<IJSVoidResult>(identifier, cancellationToken, args);
        
            /// <summary>
            /// Invokes the specified JavaScript function asynchronously.
            /// <para>
            /// <see cref="JSRuntime"/> will apply timeouts to this operation based on the value configured in <see cref="JSRuntime.DefaultAsyncTimeout"/>. To dispatch a call with a different timeout, or no timeout,
            /// consider using <see cref="InvokeAsync{TValue}(string, CancellationToken, object[])" />.
            /// </para>
            /// </summary>
            /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args)
                => InvokeAsync<TValue>(identifier, default, args);
        
            /// <summary>
            /// Invokes the specified JavaScript function asynchronously.
            /// </summary>
            /// <typeparam name="TValue">The JSON-serializable return type.</typeparam>
            /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
            /// <param name="cancellationToken">
            /// A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts
            /// (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.
            /// </param>
            /// <param name="args">JSON-serializable arguments.</param>
            /// <returns>An instance of <typeparamref name="TValue"/> obtained by JSON-deserializing the return value.</returns>
            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, params object?[]? args)
                => JsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);

        ``
        }
        `-
            #endregion
        }

        """;

    #endregion


    #region Substitution

    private const string SUMMARY_PARAMETERS = """
        ``
        foreach (TSParameter parameter in function.ParameterList) {
        `+
            /// <param name="`parameter.Name`"></param>
        ``
        }
        `-
        """;

    private const string PARAMETERS = """
        ``
        foreach (string str in parameters)
            yield return str;
        ``
        """;

    private const string ARGUMENTS = """
        ``
        foreach (string str in arguments)
            yield return str;
        ``
        """;

    private const string MAPPED_IJS_VOID_RESULT = """
        ``
        if (returnTypeMapped == "void")
            yield return "IJSVoidResult";
        else
            yield return returnTypeMapped;
        ``
        """;

    private const string MAPPED_ASYNC = """
        ``
        if (returnTypeMapped == "void")
            yield return "async ValueTask";
        else {
            yield return "ValueTask<";
            yield return returnTypeMapped;
            yield return ">";
        }
        ``
        """;

    private const string MAPPED_AWAIT = """
        ``
        if (returnTypeMapped == "void")
            yield return " await";
        ``
        """;

    #endregion
}
