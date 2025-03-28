using AssemblyVersionInfo;
using Microsoft.CodeAnalysis;
using TSRuntime.Configs;

namespace TSRuntime.Generation;

/// <summary>
/// The basic content of the interface is added:<br />
/// - PreloadAllModules()<br />
/// - JSRuntime methods (Sync/TrySync/Async)<br />
/// - internal TSInvoke-methods
/// </summary>
public static class InterfaceCoreBuilder {
    /// <summary>
    /// The basic content of the interface is added:<br />
    /// - PreloadAllModules()<br />
    /// - JSRuntime methods (Sync/TrySync/Async)<br />
    /// - internal TSInvoke-methods
    /// </summary>
    /// <param name="context"></param>
    /// <param name="configOrError"></param>
    public static void BuildInterfaceCore(SourceProductionContext context, (Config? config, Diagnostic? error) configOrError) {
        if (configOrError.error is not null)
            return;

        Config config = configOrError.config!;

        string partialOrEmpty;
        string interfaceSummary;
        if (!config.ModuleGrouping) {
            partialOrEmpty = "partial ";
            interfaceSummary = """
                /// <summary>
                /// <para>Interface for JS-interop.</para>
                /// <para>It contains an invoke-method for every js-function, a preload-method for every module and a method to load all modules.</para>
                /// </summary>
                """;
        }
        else {
            partialOrEmpty = string.Empty;
            interfaceSummary = """
                /// <summary>
                /// <para>Interface to preload all modules.</para>
                /// <para>To invoke JS-interop methods or load a specific module, use the dedicated interface for the corresponding module.</para>
                /// </summary>
                """;
        }

        const string JSRUNTIME_SYNC = """
                /// <summary>
                /// Invokes the specified JavaScript function synchronously.
                /// </summary>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="args">JSON-serializable arguments.</param>
                public void InvokeVoid(string identifier, params object?[]? args)
                    => Invoke<Infrastructure.IJSVoidResult>(identifier, args);

                /// <summary>
                /// Invokes the specified JavaScript function synchronously.
                /// </summary>
                /// <typeparam name="TResult">The JSON-serializable return type.</typeparam>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="args">JSON-serializable arguments.</param>
                /// <returns>An instance of <typeparamref name="TResult"/> obtained by JSON-deserializing the return value.</returns>
                public TResult Invoke<TResult>(string identifier, params object?[]? args);
            """;

        const string JSRUNTIME_TRYSYNC = """
                /// <summary>
                /// This method performs synchronous, if the underlying implementation supports synchrounous interoperability.
                /// </summary>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="args">JSON-serializable arguments.</param>
                /// <returns></returns>
                public async ValueTask InvokeVoidTrySync(string identifier, params object?[]? args)
                    => await InvokeTrySync<Infrastructure.IJSVoidResult>(identifier, default, args);

                /// <summary>
                /// This method performs synchronous, if the underlying implementation supports synchrounous interoperability.
                /// </summary>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <param name="args">JSON-serializable arguments.</param>
                /// <returns></returns>
                public async ValueTask InvokeVoidTrySync(string identifier, CancellationToken cancellationToken, params object?[]? args)
                    => await InvokeTrySync<Infrastructure.IJSVoidResult>(identifier, cancellationToken, args);

                /// <summary>
                /// This method performs synchronous, if the underlying implementation supports synchrounous interoperability.
                /// </summary>
                /// <typeparam name="TResult">The JSON-serializable return type.</typeparam>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="args">JSON-serializable arguments.</param>
                /// <returns>An instance of <typeparamref name="TResult"/> obtained by JSON-deserializing the return value.</returns>
                public ValueTask<TResult> InvokeTrySync<TResult>(string identifier, params object?[]? args)
                    => InvokeTrySync<TResult>(identifier, default, args);

                /// <summary>
                /// This method performs synchronous, if the underlying implementation supports synchrounous interoperability.
                /// </summary>
                /// <typeparam name="TResult">The JSON-serializable return type.</typeparam>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <param name="args">JSON-serializable arguments.</param>
                /// <returns>An instance of <typeparamref name="TResult"/> obtained by JSON-deserializing the return value.</returns>
                public ValueTask<TResult> InvokeTrySync<TResult>(string identifier, CancellationToken cancellationToken, params object?[]? args);
            """;

        const string JSRUNTIME_ASYNC = """
                /// <summary>
                /// Invokes the specified JavaScript function asynchronously.
                /// <para>
                /// <see cref="JSRuntime"/> will apply timeouts to this operation based on the value configured in <see cref="JSRuntime.DefaultAsyncTimeout"/>. To dispatch a call with a different timeout, or no timeout,
                /// consider using <see cref="InvokeVoidAsync{TResult}(string, CancellationToken, object[])" />.
                /// </para>
                /// </summary>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="args">JSON-serializable arguments.</param>
                /// <returns></returns>
                public async ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
                    => await InvokeAsync<Infrastructure.IJSVoidResult>(identifier, default, args);

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
                    => await InvokeAsync<Infrastructure.IJSVoidResult>(identifier, cancellationToken, args);

                /// <summary>
                /// Invokes the specified JavaScript function asynchronously.
                /// <para>
                /// <see cref="JSRuntime"/> will apply timeouts to this operation based on the value configured in <see cref="JSRuntime.DefaultAsyncTimeout"/>. To dispatch a call with a different timeout, or no timeout,
                /// consider using <see cref="InvokeAsync{TResult}(string, CancellationToken, object[])" />.
                /// </para>
                /// </summary>
                /// <typeparam name="TResult">The JSON-serializable return type.</typeparam>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="args">JSON-serializable arguments.</param>
                /// <returns>An instance of <typeparamref name="TResult"/> obtained by JSON-deserializing the return value.</returns>
                public ValueTask<TResult> InvokeAsync<TResult>(string identifier, params object?[]? args)
                    => InvokeAsync<TResult>(identifier, default, args);

                /// <summary>
                /// Invokes the specified JavaScript function asynchronously.
                /// </summary>
                /// <typeparam name="TResult">The JSON-serializable return type.</typeparam>
                /// <param name="identifier">An identifier for the function to invoke. For example, the value <c>"someScope.someFunction"</c> will invoke the function <c>window.someScope.someFunction</c>.</param>
                /// <param name="cancellationToken">
                /// A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts
                /// (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.
                /// </param>
                /// <param name="args">JSON-serializable arguments.</param>
                /// <returns>An instance of <typeparamref name="TResult"/> obtained by JSON-deserializing the return value.</returns>
                public ValueTask<TResult> InvokeAsync<TResult>(string identifier, CancellationToken cancellationToken, params object?[]? args);
            """;

        string jsRuntimeMethods = (config.JSRuntimeSyncEnabled, config.JSRuntimeTrySyncEnabled, config.JSRuntimeAsyncEnabled) switch {
            (true, true, true) => $"\n\n{JSRUNTIME_SYNC}\n\n{JSRUNTIME_TRYSYNC}\n\n{JSRUNTIME_ASYNC}\n",
            (true, true, false) => $"\n\n{JSRUNTIME_SYNC}\n\n{JSRUNTIME_TRYSYNC}\n",
            (true, false, true) => $"\n\n{JSRUNTIME_SYNC}\n\n{JSRUNTIME_ASYNC}\n",
            (false, true, true) => $"\n\n{JSRUNTIME_TRYSYNC}\n\n{JSRUNTIME_ASYNC}\n",
            (true, false, false) => $"\n\n{JSRUNTIME_SYNC}\n",
            (false, true, false) => $"\n\n{JSRUNTIME_TRYSYNC}\n",
            (false, false, true) => $"\n\n{JSRUNTIME_ASYNC}\n",
            (false, false, false) => ""
        };

        const string TS_INVOKE_SCRIPT = """
                /// <summary>
                /// <para>Invokes the specified JavaScript function synchronously.</para>
                /// <para>If module is not loaded or synchronous is not supported, it fails with an exception.</para>
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <returns></returns>
                protected TResult TSInvoke<TResult>(string identifier, object?[]? args);

                /// <summary>
                /// Invokes the specified JavaScript function synchronously when supported, otherwise asynchronously.
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <returns></returns>
                protected ValueTask<TResult> TSInvokeTrySync<TResult>(string identifier, object?[]? args, CancellationToken cancellationToken);

                /// <summary>
                /// Invokes the specified JavaScript function asynchronously.
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <returns></returns>
                protected ValueTask<TResult> TSInvokeAsync<TResult>(string identifier, object?[]? args, CancellationToken cancellationToken);
            """;

        const string TS_INVOKE_MODULE = """
                /// <summary>
                /// <para>Invokes the specified JavaScript function in the specified module synchronously.</para>
                /// <para>If module is not loaded or synchronous is not supported, it fails with an exception.</para>
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <param name="moduleTask">The loading task of a module</param>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <returns></returns>
                protected TResult TSInvoke<TResult>(Task<IJSObjectReference> moduleTask, string identifier, object?[]? args);

                /// <summary>
                /// Invokes the specified JavaScript function in the specified module synchronously when supported, otherwise asynchronously.
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <param name="moduleTask">The loading task of a module</param>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <returns></returns>
                protected ValueTask<TResult> TSInvokeTrySync<TResult>(Task<IJSObjectReference> moduleTask, string identifier, object?[]? args, CancellationToken cancellationToken);

                /// <summary>
                /// Invokes the specified JavaScript function in the specified module asynchronously.
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <param name="moduleTask">The loading task of a module</param>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <returns></returns>
                protected ValueTask<TResult> TSInvokeAsync<TResult>(Task<IJSObjectReference> moduleTask, string identifier, object?[]? args, CancellationToken cancellationToken);
            """;

        const string TS_INVOKE_SCRIPT_CALLBACK = """
                /// <summary>
                /// <para>Invokes the specified JavaScript function synchronously.</para>
                /// <para>If module is not loaded or synchronous is not supported, it fails with an exception.</para>
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <typeparam name="TCallback"></typeparam>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="dotNetObjectReference">reference to a csharp object with callback functions</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <returns></returns>
                protected TResult TSInvoke<TResult, TCallback>(string identifier, DotNetObjectReference<TCallback> dotNetObjectReference, object?[]? args) where TCallback : class;

                /// <summary>
                /// Invokes the specified JavaScript function synchronously when supported, otherwise asynchronously.
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <typeparam name="TCallback"></typeparam>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="dotNetObjectReference">reference to a csharp object with callback functions</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <returns></returns>
                protected ValueTask<TResult> TSInvokeTrySync<TResult, TCallback>(string identifier, DotNetObjectReference<TCallback> dotNetObjectReference, object?[]? args, CancellationToken cancellationToken) where TCallback : class;

                /// <summary>
                /// Invokes the specified JavaScript function asynchronously.
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <typeparam name="TCallback"></typeparam>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="dotNetObjectReference">reference to a csharp object with callback functions</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <returns></returns>
                protected ValueTask<TResult> TSInvokeAsync<TResult, TCallback>(string identifier, DotNetObjectReference<TCallback> dotNetObjectReference, object?[]? args, CancellationToken cancellationToken) where TCallback : class;
            """;

        const string TS_INVOKE_MODULE_CALLBACK = """
                /// <summary>
                /// <para>Invokes the specified JavaScript function in the specified module synchronously.</para>
                /// <para>If module is not loaded or synchronous is not supported, it fails with an exception.</para>
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <typeparam name="TCallback"></typeparam>
                /// <param name="moduleTask">The loading task of a module</param>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="dotNetObjectReference">reference to a csharp object with callback functions</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <returns></returns>
                protected TResult TSInvoke<TResult, TCallback>(Task<IJSObjectReference> moduleTask, string identifier, DotNetObjectReference<TCallback> dotNetObjectReference, object?[]? args) where TCallback : class;

                /// <summary>
                /// Invokes the specified JavaScript function in the specified module synchronously when supported, otherwise asynchronously.
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <typeparam name="TCallback"></typeparam>
                /// <param name="moduleTask">The loading task of a module</param>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="dotNetObjectReference">reference to a csharp object with callback functions</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <returns></returns>
                protected ValueTask<TResult> TSInvokeTrySync<TResult, TCallback>(Task<IJSObjectReference> moduleTask, string identifier, DotNetObjectReference<TCallback> dotNetObjectReference, object?[]? args, CancellationToken cancellationToken) where TCallback : class;

                /// <summary>
                /// Invokes the specified JavaScript function in the specified module asynchronously.
                /// </summary>
                /// <typeparam name="TResult"></typeparam>
                /// <typeparam name="TCallback"></typeparam>
                /// <param name="moduleTask">The loading task of a module</param>
                /// <param name="identifier">name of the javascript function</param>
                /// <param name="dotNetObjectReference">reference to a csharp object with callback functions</param>
                /// <param name="args">parameter passing to the JS-function</param>
                /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                /// <returns></returns>
                protected ValueTask<TResult> TSInvokeAsync<TResult, TCallback>(Task<IJSObjectReference> moduleTask, string identifier, DotNetObjectReference<TCallback> dotNetObjectReference, object?[]? args, CancellationToken cancellationToken) where TCallback : class;
            """;


        string source = $$"""
            // <auto-generated/>
            #pragma warning disable
            #nullable enable annotations


            using System.Threading;
            using System.Threading.Tasks;

            namespace Microsoft.JSInterop;

            {{interfaceSummary}}
            [System.CodeDom.Compiler.GeneratedCodeAttribute("{{Assembly.NAME}}", "{{Assembly.VERSION_MAJOR_MINOR_BUILD}}")]
            public {{partialOrEmpty}}interface ITSRuntime {
                /// <summary>
                /// <para>Fetches all modules as javascript-modules.</para>
                /// <para>If already loading, it doesn't trigger a second loading and if any already loaded, these are not loaded again, so if all already loaded, it returns a completed task.</para>
                /// </summary>
                /// <returns>A Task that will complete when all module loading Tasks have completed.</returns>
                public Task {{config.PreloadAllModulesName}}();
            {{jsRuntimeMethods}}


            {{TS_INVOKE_SCRIPT}}


            {{TS_INVOKE_MODULE}}


            {{TS_INVOKE_SCRIPT_CALLBACK}}


            {{TS_INVOKE_MODULE_CALLBACK}}
            }

            """;

        context.AddSource("ITSRuntime_Core.g.cs", source);
    }
}
