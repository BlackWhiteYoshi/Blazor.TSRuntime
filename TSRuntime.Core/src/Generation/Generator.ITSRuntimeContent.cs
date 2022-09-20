using TSRuntime.Core.Configs;
using TSRuntime.Core.Parsing;

namespace TSRuntime.Core.Generation;

public static partial class Generator {
    public static IEnumerable<string> GetITSRuntimeContent(SyntaxTree syntaxTree, Config config) {
        yield return """
            // --- <auto generated> ---


            """;
        foreach (string usingStatement in config.UsingStatements) {
            yield return "using ";
            yield return usingStatement;
            yield return """
                ;

                """;
        }
        yield return """
            using Microsoft.JSInterop.Infrastructure;
            using System.Diagnostics.CodeAnalysis;
            using System.Threading;
            using System.Threading.Tasks;
            
            namespace Microsoft.JSInterop;

            public interface ITSRuntime {
                protected const int MODULE_COUNT = 
            """;
        yield return syntaxTree.ModuleList.Count.ToString();
        yield return """
            ;

                protected IJSRuntime JsRuntime { get; }


            
            """;


        for (int i = 0; i < syntaxTree.ModuleList.Count; i++) {
            TSModule module = syntaxTree.ModuleList[i];
            string index = i.ToString();

            yield return """
                    /// <summary>
                    /// <para>Preloads 
                """;
            yield return module.ModuleName;
            yield return " (";
            yield return module.ModulePath;
            yield return """
                ) as javascript-module.</para>
                    /// <para>If already loading, it doesn't trigger a second loading and if already loaded, it returns synchronously with a completed task.</para>
                    /// </summary>
                    public async ValueTask PreLoad_
                """;
            yield return module.ModuleName;
            yield return """
                ()
                        => await GetOrLoadModule(
                """;
            yield return index;
            yield return @", """;
            yield return module.ModulePath;
            yield return """
                ");


                """;
        }

        yield return """

                protected Task<IJSObjectReference> GetOrLoadModule(int index, string url);


                #region module methods


            """;

        for (int i = 0; i < syntaxTree.ModuleList.Count; i++) {
            TSModule module = syntaxTree.ModuleList[i];
            string index = i.ToString();

            foreach (TSFunction function in module.FunctionList) {
                // Summary parameterList
                static IEnumerable<string> SummaryParameters(List<TSParameter> parameterList) {
                    foreach (TSParameter parameter in parameterList) {
                        yield return """
                                /// <param name="
                            """;
                        yield return parameter.Name;
                        yield return """
                            "></param>
                            
                            """;
                    }
                }

                // mapping returnType
                bool returnTypeMapSuccess = config.TypeMap.TryGetValue(function.ReturnType.Type.ToString(), out string? returnTypeString);
                string returnTypeMapped = returnTypeMapSuccess switch {
                    true => returnTypeString!,
                    false => function.ReturnType.Type
                };

                // paramter string list
                List<string> parameters = new(function.ParameterList.Count * 4);
                List<string> arguments = new(function.ParameterList.Count * 2);
                if (function.ParameterList.Count > 0) {
                    foreach (TSParameter parameter in function.ParameterList) {
                        bool parameterTypeMapSuccess = config.TypeMap.TryGetValue(parameter.Type.ToString(), out string? parameterTypeString);
                        string mappedType = parameterTypeMapSuccess switch {
                            true => parameterTypeString!,
                            false => parameter.Type
                        };

                        parameters.Add(mappedType);
                        if (parameter.TypeNullable)
                            parameters.Add("?");
                        if (parameter.Array)
                            parameters.Add("[]");
                        if (parameter.ArrayNullable)
                            parameters.Add("?");
                        parameters.Add(" ");
                        parameters.Add(parameter.Name);
                        parameters.Add(", ");

                        arguments.Add(parameter.Name);
                        arguments.Add(", ");
                    }
                    parameters.RemoveAt(parameters.Count - 1);
                    arguments.RemoveAt(arguments.Count - 1);
                }


                if (!function.ReturnPromise) {
                    // Invoke without (out bool success)
                    yield return """
                            /// <summary>
                            /// <para>Invokes in module 
                        """;
                    yield return module.ModuleName;
                    yield return " the js-function ";
                    yield return function.Name;
                    yield return """
                        synchronously.</para>
                            /// <para>If module is not loaded, it returns without any invoking. If synchronous is not supported, it fails with an exception.</para>
                            /// </summary>
                        
                        """;
                    foreach (string str in SummaryParameters(function.ParameterList))
                        yield return str;
                    yield return """
                            /// <returns>default when the module is not loaded, otherwise result of the js-function</returns>
                            public 
                        """;
                    yield return returnTypeMapped;
                    yield return " Invoke_";
                    yield return module.ModuleName;
                    yield return "_";
                    yield return function.Name;
                    yield return "(";
                    foreach (string str in parameters)
                        yield return str;
                    yield return """
                        )
                                => Invoke<
                        """;
                    if (returnTypeMapped == "void")
                        yield return "IJSVoidResult";
                    else
                        yield return returnTypeMapped;
                    yield return @">(";
                    yield return index;
                    yield return @", """;
                    yield return module.ModulePath;
                    yield return @""", """;
                    yield return function.Name;
                    yield return @""", out _";
                    if (arguments.Count > 0) {
                        yield return ", ";
                        foreach (string str in arguments)
                            yield return str;
                    }
                    yield return """
                        );
                        """;


                    // Invoke with (out bool success)
                    yield return """


                            /// <summary>
                            /// <para>Invokes in module 《module.Name》 the js-function 《function.Name》 synchronously.</para>
                            /// <para>If module is not loaded, it returns without any invoking. If synchronous is not supported, it fails with an exception.</para>
                            /// </summary>
                        
                        """;
                    foreach (string str in SummaryParameters(function.ParameterList))
                        yield return str;
                    yield return """
                            /// <param name="success">false when the module is not loaded, otherwise true</param>
                            /// <returns>default when the module is not loaded, otherwise result of the js-function</returns>
                            public 
                        """;
                    yield return returnTypeMapped;
                    yield return " Invoke_";
                    yield return module.ModuleName;
                    yield return "_";
                    yield return function.Name;
                    yield return "(";
                    if (parameters.Count > 0) {
                        foreach (string str in parameters)
                            yield return str;
                        yield return ", ";
                    }
                    yield return """
                        out bool success)
                                => Invoke<
                        """;
                    if (returnTypeMapped == "void")
                        yield return "IJSVoidResult";
                    else
                        yield return returnTypeMapped;
                    yield return @">(";
                    yield return index;
                    yield return @", """;
                    yield return module.ModulePath;
                    yield return @""", """;
                    yield return function.Name;
                    yield return @""", out success";
                    if (arguments.Count > 0) {
                        yield return ", ";
                        foreach (string str in arguments)
                            yield return str;
                    }
                    yield return """
                        );
                        """;


                    // TrySync
                    yield return """
                        

                            /// <summary>
                            /// Invokes in module 《module.Name》 the js-function 《function.Name》 synchronously when supported, otherwise asynchronously.
                            /// </summary>
                        
                        """;
                    foreach (string str in SummaryParameters(function.ParameterList))
                        yield return str;
                    yield return """
                            /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                            /// <returns></returns>
                            public 
                        """;
                    if (returnTypeMapped == "void") {
                        yield return "async ValueTask";
                    }
                    else {
                        yield return "ValueTask<";
                        yield return returnTypeMapped;
                        yield return ">";
                    }
                    yield return " InvokeTrySync_";
                    yield return module.ModuleName;
                    yield return "_";
                    yield return function.Name;
                    yield return "(";
                    if (parameters.Count > 0) {
                        foreach (string str in parameters)
                            yield return str;
                        yield return ", ";
                    }
                    yield return """
                        CancellationToken cancellationToken = default)
                                => 
                        """;
                    if (returnTypeMapped == "void")
                        yield return "await InvokeTrySync<IJSVoidResult";
                    else {
                        yield return "InvokeTrySync<";
                        yield return returnTypeMapped;
                    }
                    yield return @">(";
                    yield return index;
                    yield return @", """;
                    yield return module.ModulePath;
                    yield return @""", """;
                    yield return function.Name;
                    yield return @""", cancellationToken";
                    if (arguments.Count > 0) {
                        yield return ", ";
                        foreach (string str in arguments)
                            yield return str;
                    }
                    yield return """
                        );
                        """;
                }


                // Async
                yield return """


                        /// <summary>
                        /// Invokes in module 《module.Name》 the js-function 《function.Name》 asynchronously.
                        /// </summary>
                    
                    """;
                foreach (string str in SummaryParameters(function.ParameterList))
                    yield return str;
                yield return """
                        /// <param name="cancellationToken">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref="JSRuntime.DefaultAsyncTimeout"/>) from being applied.</param>
                        /// <returns></returns>
                        public 
                    """;
                if (returnTypeMapped == "void") {
                    yield return "async ValueTask";
                }
                else {
                    yield return "ValueTask<";
                    yield return returnTypeMapped;
                    yield return ">";
                }
                yield return " InvokeAsync_";
                yield return module.ModuleName;
                yield return "_";
                yield return function.Name;
                yield return "(";
                if (parameters.Count > 0) {
                    foreach (string str in parameters)
                        yield return str;
                    yield return ", ";
                }
                yield return """
                    CancellationToken cancellationToken = default)
                            => 
                    """;
                if (returnTypeMapped == "void")
                    yield return "await InvokeAsync<IJSVoidResult";
                else {
                    yield return "InvokeAsync<";
                    yield return returnTypeMapped;
                }
                yield return @">(";
                yield return index;
                yield return @", """;
                yield return module.ModulePath;
                yield return @""", """;
                yield return function.Name;
                yield return @""", cancellationToken";
                if (arguments.Count > 0) {
                    yield return ", ";
                    foreach (string str in arguments)
                        yield return str;
                }
                yield return """
                    );



                    """;
            }
        }
        
        yield return """

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

                #endregion


                #region non-module methods

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

                #endregion
            }
            
            """;
    }
}
