using AssemblyVersionInfo;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.ObjectPool;
using System.Text;
using TSRuntime.Configs;
using TSRuntime.Parsing;

namespace TSRuntime.Generation;

/// <summary>
/// <para>Cotains 2 methods for building the interface for JS-module and JS-script.</para>
/// <para>
/// JS-module is a JS file that is loaded via "import" command.<br />
/// JS-script is a JS file that is loaded via "&lt;script&gt;" tag.
/// </para>
/// </summary>
public static class InterfaceBuilder {
    /// <summary>
    /// Builds the content of a module:
    /// - Preload-method<br />
    /// - module methods
    /// </summary>
    /// <param name="stringBuilderPool"></param>
    /// <param name="context"></param>
    /// <param name="parameters"></param>
    public static void BuildInterfaceModule(this ObjectPool<StringBuilder> stringBuilderPool, SourceProductionContext context, (TSModule module, Config config) parameters) {
        StringBuilder builder = stringBuilderPool.Get();

        (string hintName, string source) = new BuildInterfaceFileCore(builder, parameters.config).Build(parameters.module);

        context.AddSource(hintName, source);
        stringBuilderPool.Return(builder);
    }

    /// <summary>
    /// Builds the content of a script file by creating wrappers for each function.
    /// </summary>
    /// <param name="stringBuilderPool"></param>
    /// <param name="context"></param>
    /// <param name="parameters"></param>
    public static void BuildInterfaceScript(this ObjectPool<StringBuilder> stringBuilderPool, SourceProductionContext context, (TSScript script, Config config) parameters) {
        StringBuilder builder = stringBuilderPool.Get();

        (string hintName, string source) = new BuildInterfaceFileCore(builder, parameters.config).Build(parameters.script);

        context.AddSource(hintName, source);
        stringBuilderPool.Return(builder);
    }

    /// <summary>
    /// Datastructure used in <see cref="BuildInterfaceModule"/> and <see cref="BuildInterfaceScript"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="config"></param>
    private struct BuildInterfaceFileCore(StringBuilder builder, Config config) {
        private string scriptName = default!;
        private bool isModule = default;
        private TSFunction function = default!;
        private readonly List<(MappedType parameter, MappedType[]? callback)> mappedParameterList = [];
        private MappedType mappedReturnType = default;
        private readonly List<GenericType> genericParameterList = [];
        private readonly List<GenericType> genericCallbackList = [];
        private string returnType = default!;
        private string returnModifiers = default!;


        /// <summary>
        /// Builds the interface for a module.
        /// </summary>
        /// <returns></returns>
        public (string hintName, string source) Build(TSModule module) {
            scriptName = module.Name;
            isModule = true;

            AppendUsingsAndNamespace();

            // head
            if (!config.ModuleGrouping)
                builder.Append("public partial interface ITSRuntime {\n");
            else {
                builder.AppendInterpolation($"""
                    /// <summary>
                    /// <para>Interface to JS-interop the module '{module.Name}'.</para>
                    /// <para>It contains invoke-methods for the js-functions and a preload-method for loading the module of '{module.Name}'.</para>
                    /// </summary>
                    [System.CodeDom.Compiler.GeneratedCodeAttribute("{Assembly.NAME}", "{Assembly.VERSION_MAJOR_MINOR_BUILD}")]
                    public interface 
                    """);
                config.ModuleGroupingNamePattern.AppendNaming(builder, module.Name);
                builder.Append(" : ITSRuntime {\n");
            }

            // preload module
            {
                builder.AppendInterpolation($"""
                        protected Task<IJSObjectReference> Get{module.Name}Module();

                        /// <summary>
                        /// <para>Loads '{module.Name}' ({module.URLPath}) as javascript-module.</para>
                        /// <para>If already loading, it does not trigger a second loading and if already loaded, it returns a completed task.</para>
                        /// </summary>
                        /// <returns>A Task that will complete when the module import have completed.</returns>
                        public Task 
                    """);
                config.PreloadNamePattern.AppendNaming(builder, module.Name);
                builder.AppendInterpolation($"() => Get{module.Name}Module();\n\n\n");
            }

            AppendFunctionList(module.FunctionList);

            builder.Length -= 2;
            builder.Append("}\n");


            string source = builder.ToString();

            builder.Clear();
            if (!config.ModuleGrouping)
                builder.AppendInterpolation($"ITSRuntime_{module.Name}");
            else
                config.ModuleGroupingNamePattern.AppendNaming(builder, module.Name);
            builder.Append(".g.cs");
            string hintName = builder.ToString();

            return (hintName, source);
        }

        /// <summary>
        /// Builds the interface for a script.
        /// </summary>
        /// <returns></returns>
        public (string hintName, string source) Build(TSScript script) {
            scriptName = script.Name;
            isModule = false;


            AppendUsingsAndNamespace();

            // head
            builder.Append("public partial interface ITSRuntime {\n");

            AppendFunctionList(script.FunctionList);

            if (builder[^2] is '\n')
                builder.Length -= 2;
            builder.Append("}\n");


            string source = builder.ToString();
            string hintName = $"ITSRuntime_{scriptName}.g.cs";
            return (hintName, source);
        }


        /// <summary>
        /// Builds top declaration of the source file<br />
        /// Ends with 2 line breaks.
        /// </summary>
        private readonly void AppendUsingsAndNamespace() {
            builder.Append("""
            // <auto-generated/>
            #pragma warning disable
            #nullable enable annotations


            using System.Threading;
            using System.Threading.Tasks;

            """);

            foreach (string usingStatement in config.UsingStatements)
                builder.AppendInterpolation($"using {usingStatement};\n");
            builder.Append('\n');

            builder.Append("namespace Microsoft.JSInterop;\n\n");
        }

        /// <summary>
        /// <para>
        /// For each function it initializes the paramters (<see cref="mappedParameterList"/>, <see cref="mappedReturnType"/>, <see cref="genericParameterList"/>, <see cref="returnType"/>, <see cref="returnModifiers"/>)<br />
        /// and calls the <see cref="AppendInvokeSyncMethod"/> / <see cref="AppendInvokeAsyncMethod"/>.
        /// </para>
        /// Ends with 3 line break.
        /// </summary>
        /// <param name="functionList"></param>
        /// <param name="isModule"></param>
        private void AppendFunctionList(IReadOnlyList<TSFunction> functionList) {
            if (functionList.Count > 0) {
                for (int i = 0; i < functionList.Count; i++) {
                    function = functionList[i];

                    mappedParameterList.Clear();
                    for (int j = 0; j < function.ParameterList.Length; j++) {
                        string? type = function.ParameterList[j].type;
                        if (type is not null)
                            if (config.TypeMap.TryGetValue(type, out MappedType mappedType))
                                mappedParameterList.Add((mappedType, null));
                            else
                                mappedParameterList.Add((new MappedType(type), null));
                        else {
                            MappedType[] mappedCallbackTypeList = new MappedType[function.ParameterList[j].typeCallback.Length];

                            for (int k = 0; k < function.ParameterList[j].typeCallback.Length; k++) {
                                string callbackType = function.ParameterList[j].typeCallback[k].type ?? "CALLBACK_INSIDE_CALLBACK_NOT_SUPPORTED";
                                if (config.TypeMap.TryGetValue(callbackType, out MappedType mappedType))
                                    mappedCallbackTypeList[k] = new MappedType(mappedType.Type, mappedType.GenericTypes);
                                else
                                    mappedCallbackTypeList[k] = new MappedType(callbackType);
                            }

                            mappedParameterList.Add((default, mappedCallbackTypeList));
                        }
                    }

                    genericCallbackList.Clear();
                    for (int j = 0; j < function.ParameterList.Length; j++)
                        if (mappedParameterList[j].callback is not null)
                            for (int k = 0; k < mappedParameterList[j].callback!.Length; k++)
                                for (int l = 0; l < mappedParameterList[j].callback![k].GenericTypes.Length; l++)
                                    if (!genericCallbackList.Contains(mappedParameterList[j].callback![k].GenericTypes[l]))
                                        genericCallbackList.Add(mappedParameterList[j].callback![k].GenericTypes[l]);

                    string rawReturnType = function.ReturnType.type ?? "CALLBACK_RETURN_TYPE_NOT_SUPPORTED";
                    if (!config.TypeMap.TryGetValue(rawReturnType, out mappedReturnType))
                        mappedReturnType = new(rawReturnType);

                    returnType = mappedReturnType.Type;
                    returnModifiers = (function.ReturnType.typeNullable, function.ReturnType.array, function.ReturnType.arrayNullable) switch {
                        (false, false, _) => string.Empty,
                        (true, false, _) => "?",
                        (false, true, false) => "[]",
                        (false, true, true) => "[]?",
                        (true, true, false) => "?[]",
                        (true, true, true) => "?[]?"
                    };

                    // private callback class
                    if (function.HasCallback) {
                        // attributes
                        for (int k = 0; k < function.ParameterList.Length; k++)
                            if (function.ParameterList[k].typeCallback.Length > 0)
                                builder.AppendInterpolation($"    [method: DynamicDependency(nameof(_{function.ParameterList[k].name}))]\n");


                        // class head

                        builder.Append("    private sealed class ");
                        config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, string.Empty);
                        builder.Append("Callback");

                        // generics
                        if (genericCallbackList.Count > 0) {
                            builder.Append('<');

                            builder.Append(genericCallbackList[0].Name);
                            for (int k = 1; k < genericCallbackList.Count; k++)
                                builder.AppendInterpolation($", {genericCallbackList[k].Name}");

                            builder.Append('>');
                        }

                        // generic constraints
                        foreach (GenericType genericType in genericCallbackList)
                            if (genericType.Constraint is not null)
                                builder.AppendInterpolation($" where {genericType.Name} : {genericType.Constraint}");

                        builder.Append(" {\n");


                        // callbacks
                        for (int k = 0; k < function.ParameterList.Length; k++)
                            if (mappedParameterList[k].callback is MappedType[] callbackTypeList) {
                                // delegate variable
                                builder.Append("        public ");
                                {
                                    if (function.ParameterList[k].typeCallback[^1].type is "void" && !function.ParameterList[k].typeCallbackPromise) {
                                        builder.Append("Action");

                                        if (callbackTypeList.Length > 1) {
                                            builder.Append('<');

                                            builder.Append(callbackTypeList[0].Type);
                                            for (int l = 1; l < callbackTypeList.Length - 1; l++) // last parameter is returnType
                                                builder.AppendInterpolation($", {callbackTypeList[l].Type}");

                                            builder.Append('>');
                                        }
                                    }
                                    else {
                                        builder.Append("Func<");

                                        for (int l = 0; l < callbackTypeList.Length - 1; l++)
                                            builder.AppendInterpolation($"{callbackTypeList[l].Type}, ");

                                        if (!function.ParameterList[k].typeCallbackPromise)
                                            builder.Append(callbackTypeList[^1].Type);
                                        else
                                            if (function.ParameterList[k].typeCallback[^1].type is "void")
                                                builder.Append("ValueTask");
                                            else
                                                builder.AppendInterpolation($"ValueTask<{callbackTypeList[^1].Type}>");

                                        builder.Append('>');
                                    }
                                }
                                builder.AppendInterpolation($" _{function.ParameterList[k].name};\n");

                                // [JSInvokable] method
                                {
                                    builder.Append("        [JSInvokable] public ");
                                    if (!function.ParameterList[k].typeCallbackPromise)
                                        builder.Append(callbackTypeList[^1].Type);
                                    else
                                        if (function.ParameterList[k].typeCallback[^1].type is "void")
                                            builder.Append("ValueTask");
                                        else
                                            builder.AppendInterpolation($"ValueTask<{callbackTypeList[^1].Type}>");

                                    // parameter
                                    builder.AppendInterpolation($" {function.ParameterList[k].name}(");
                                    for (int l = 0; l < callbackTypeList.Length - 1; l++) // last parameter is returnType
                                        builder.AppendInterpolation($"{callbackTypeList[l].Type} {function.ParameterList[k].typeCallback[l].name}, ");
                                    if (builder[^1] is ' ')
                                        builder.Length -= 2;

                                    // arguments
                                    builder.AppendInterpolation($") => _{function.ParameterList[k].name}(");
                                    for (int l = 0; l < function.ParameterList[k].typeCallback.Length - 1; l++) // last parameter is returnType
                                        builder.AppendInterpolation($"{function.ParameterList[k].typeCallback[l].name}, ");
                                    if (builder[^1] is ' ')
                                        builder.Length -= 2;

                                    builder.Append(");\n\n");
                                }
                            }

                        builder.Length--;
                        builder.Append("    }\n");
                    }

                    if (function.ReturnPromise && config.PromiseOnlyAsync)
                        AppendInvokeAsyncMethod();
                    else {
                        if (config.InvokeFunctionSyncEnabled)
                            AppendInvokeSyncMethod();
                        if (config.InvokeFunctionTrySyncEnabled)
                            AppendInvokeTrySyncMethod();
                        if (config.InvokeFunctionAsyncEnabled)
                            AppendInvokeAsyncMethod();
                    }
                }

                builder.Append('\n');
            }
        }


        /// <summary>
        /// <para>Appends the invoke method for a js function for sync invoke calls.</para>
        /// <para>
        /// If the parameterlist of the js function ends on optional variables, corresponding overload methods are created.<br />
        /// In this case multiple methods for that js function are appended.
        /// </para>
        /// </summary>
        private readonly void AppendInvokeSyncMethod()
            => AppendInvokeMethodCore(config.InvokeFunctionActionNameSync, "synchronously.", "TSInvoke", isSync: true);

        /// <summary>
        /// <para>Appends the invoke method for a js function for sync invoke calls when supported, otherwise async.</para>
        /// <para>
        /// If the parameterlist of the js function ends on optional variables, corresponding overload methods are created.<br />
        /// In this case multiple methods for that js function are appended.
        /// </para>
        /// </summary>
        private readonly void AppendInvokeTrySyncMethod()
            => AppendInvokeMethodCore(config.InvokeFunctionActionNameTrySync, "synchronously when supported, otherwise asynchronously.", "TSInvokeTrySync", isSync: false);

        /// <summary>
        /// <para>Appends the invoke method for a js function for async invoke calls.</para>
        /// <para>
        /// If the parameterlist of the js function ends on optional variables, corresponding overload methods are created.<br />
        /// In this case multiple methods for that js function are appended.
        /// </para>
        /// </summary>
        private readonly void AppendInvokeAsyncMethod()
            => AppendInvokeMethodCore(config.InvokeFunctionActionNameAsync, "asynchronously.", "TSInvokeAsync", isSync: false);

        /// <summary>
        /// Core method for <see cref="AppendInvokeSyncMethod"/>, <see cref="AppendInvokeTrySyncMethod"/>, <see cref="AppendInvokeAsyncMethod"/>.
        /// </summary>
        /// <param name="invokeFunctionActionName">config.InvokeFunctionActionNameSync, config.InvokeFunctionActionNameTrySync, config.InvokeFunctionActionNameAsync</param>
        /// <param name="summaryAction">"synchronously." or "synchronously when supported, otherwise asynchronously." or "asynchronously."</param>
        /// <param name="invokeFunction">"TSInvoke" or "TSInvokeTrySync" or "TSInvokeAsync"</param>
        /// <param name="isSync">sync: true, trysync/async: false</param>
        private readonly void AppendInvokeMethodCore(string invokeFunctionActionName, string summaryAction, string invokeFunction, bool isSync) {
            int lastIndex = function.ParameterList.Length;
            do {
                lastIndex--;

                genericParameterList.Clear();
                genericParameterList.AddRange(genericCallbackList);
                for (int i = 0; i <= lastIndex; i++)
                    if (mappedParameterList[i].callback is null)
                        for (int j = 0; j < mappedParameterList[i].parameter.GenericTypes.Length; j++)
                            if (!genericParameterList.Contains(mappedParameterList[i].parameter.GenericTypes[j]))
                                genericParameterList.Add(mappedParameterList[i].parameter.GenericTypes[j]);
                for (int i = 0; i < mappedReturnType.GenericTypes.Length; i++)
                    if (!genericParameterList.Contains(mappedReturnType.GenericTypes[i]))
                        genericParameterList.Add(mappedReturnType.GenericTypes[i]);

                // <summary>
                {
                    builder.Append("    /// <summary>\n");

                    if (function.Summary != string.Empty)
                        builder.AppendInterpolation($"    /// <para>{function.Summary}</para>\n");

                    if (isModule) {
                        builder.AppendInterpolation($"    /// <para>Invokes in module '{scriptName}' the JS-function '{function.Name}' {summaryAction}</para>\n");
                        if (isSync)
                            builder.Append("    /// <para>If module is not loaded or synchronous is not supported, it fails with an exception.</para>\n");
                    }
                    else
                        builder.AppendInterpolation($"    /// <para>Invokes in script '{scriptName}' the JS-function '{function.Name}' {summaryAction}</para>\n");

                    builder.Append("    /// </summary>\n");
                }
                // <remarks>
                if (function.Remarks != string.Empty)
                    builder.AppendInterpolation($"    /// <remarks>{function.Remarks}</remarks>\n");
                // <typeparam>
                foreach (GenericType genericType in genericParameterList)
                    builder.AppendInterpolation($"    /// <typeparam name=\"{genericType.Name}\"></typeparam>\n");
                foreach ((string type, string description) in function.Generics)
                    builder.AppendInterpolation($"    /// <typeparam name=\"{type}\">{description}</typeparam>\n");
                // <param>
                for (int i = 0; i <= lastIndex; i++)
                    builder.AppendInterpolation($"    /// <param name=\"{function.ParameterList[i].name}\">{function.ParameterList[i].summary}</param>\n");
                if (!isSync)
                    builder.Append("    /// <param name=\"cancellationToken\">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref=\"JSRuntime.DefaultAsyncTimeout\"/>) from being applied.</param>\n");
                // <returns>
                if (function.ReturnType.summary != string.Empty)
                    builder.AppendInterpolation($"    /// <returns>{function.ReturnType.summary}</returns>\n");
                else if (returnType != "void")
                    builder.Append("    /// <returns>Result of the JS-function.</returns>\n");
                else if (!isSync)
                    builder.Append("    /// <returns>A Task that will complete when the JS-Function have completed.</returns>\n");

                // method head (visibility and returnType)
                if (isSync)
                    builder.AppendInterpolation($"    public {returnType}{returnModifiers} ");
                else {
                    builder.Append("    public async ValueTask");
                    if (returnType is not "void")
                        builder.AppendInterpolation($"<{returnType}{returnModifiers}>");
                    builder.Append(' ');
                }

                // method name
                config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, invokeFunctionActionName);
                if (function.ReturnPromise && config.PromiseAppendAsync)
                    builder.Append("Async");

                // generic parameters
                if (genericParameterList.Count > 0 || function.Generics.Length > 0) {
                    builder.Append('<');

                    for (int i = 0; i < genericParameterList.Count; i++)
                        builder.AppendInterpolation($"{genericParameterList[i].Name}, ");
                    foreach ((string genericType, _) in function.Generics)
                        builder.AppendInterpolation($"{genericType}, ");
                    builder.Length -= 2;

                    builder.Append('>');
                }

                // parameters
                builder.Append('(');
                if (lastIndex >= 0) {
                    for (int i = 0; i <= lastIndex; i++) {
                        if (mappedParameterList[i].callback is null)
                            builder.Append(mappedParameterList[i].parameter.Type);
                        else {
                            MappedType[] callbackTypeList = mappedParameterList[i].callback!;
                            if (function.ParameterList[i].typeCallback[^1].type is "void" && !function.ParameterList[i].typeCallbackPromise) {
                                builder.Append("Action");

                                if (callbackTypeList.Length > 1) {
                                    builder.Append('<');

                                    builder.Append(callbackTypeList[0].Type);
                                    for (int j = 1; j < callbackTypeList.Length - 1; j++) // last parameter is returnType
                                        builder.AppendInterpolation($", {callbackTypeList[j].Type}");

                                    builder.Append('>');
                                }
                            }
                            else {
                                builder.Append("Func<");

                                for (int j = 0; j < callbackTypeList.Length - 1; j++)
                                    builder.AppendInterpolation($"{callbackTypeList[j].Type}, ");

                                if (!function.ParameterList[i].typeCallbackPromise)
                                    builder.Append(callbackTypeList[^1].Type);
                                else
                                    if (function.ParameterList[i].typeCallback[^1].type is "void")
                                        builder.Append("ValueTask");
                                    else
                                        builder.AppendInterpolation($"ValueTask<{callbackTypeList[^1].Type}>");

                                builder.Append('>');
                            }
                        }
                        if (function.ParameterList[i].typeNullable)
                            builder.Append('?');
                        if (function.ParameterList[i].array)
                            builder.Append("[]");
                        if (function.ParameterList[i].arrayNullable)
                            builder.Append('?');

                        builder.AppendInterpolation($" {function.ParameterList[i].name}, ");
                    }
                    if (isSync)
                        builder.Length -= 2;
                }
                if (!isSync)
                    builder.Append("CancellationToken cancellationToken = default");
                builder.Append(')');

                // generic constraints
                foreach (GenericType genericType in genericParameterList)
                    if (genericType.Constraint is not null)
                        builder.AppendInterpolation($" where {genericType.Name} : {genericType.Constraint}");

                builder.Append(" {\n");


                // body
                if (function.HasCallback) {
                    builder.Append("        using DotNetObjectReference<");

                    // type declaration of variable
                    config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, string.Empty);
                    builder.Append("Callback");
                    if (genericCallbackList.Count > 0) {
                        builder.Append('<');

                        builder.Append(genericCallbackList[0].name);
                        for (int i = 1; i < genericCallbackList.Count; i++)
                            builder.AppendInterpolation($", {genericCallbackList[i].name}");

                        builder.Append('>');
                    }

                    // new() call
                    builder.Append("> __callback = DotNetObjectReference.Create(new ");
                    config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, string.Empty);
                    builder.Append("Callback");
                    if (genericCallbackList.Count > 0) {
                        builder.Append('<');

                        builder.Append(genericCallbackList[0].name);
                        for (int i = 1; i < genericCallbackList.Count; i++)
                            builder.AppendInterpolation($", {genericCallbackList[i].name}");

                        builder.Append('>');
                    }

                    // setting parameters in the initializer
                    {
                        builder.Append("() { ");

                        for (int i = 0; i < function.ParameterList.Length; i++)
                            if (function.ParameterList[i].typeCallback.Length > 0)
                                builder.AppendInterpolation($"_{function.ParameterList[i].name} = {function.ParameterList[i].name}, ");
                        builder.Length -= 2;

                        builder.Append(" });\n");
                    }
                }

                builder.Append("        ");
                if (returnType is not "void")
                    builder.Append("return ");
                if (!isSync)
                    builder.Append("await ");
                builder.Append(invokeFunction);

                // generics of TSInvoke<...>
                {
                    builder.Append('<');

                    if (returnType is "void")
                        builder.Append("Infrastructure.IJSVoidResult");
                    else
                        builder.AppendInterpolation($"{returnType}{returnModifiers}");

                    if (function.HasCallback) {
                        builder.Append(", ");

                        config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, string.Empty);
                        builder.Append("Callback");
                        if (genericCallbackList.Count > 0) {
                            builder.Append('<');

                            builder.Append(genericCallbackList[0].name);
                            for (int i = 1; i < genericCallbackList.Count; i++)
                                builder.AppendInterpolation($", {genericCallbackList[i].name}");

                            builder.Append('>');
                        }
                    }

                    builder.Append(">(");
                }

                // arguments of TSInvoke
                {
                    if (isModule)
                        builder.AppendInterpolation($"Get{scriptName}Module(), ");

                    builder.AppendInterpolation($"\"{function.Name}\", ");

                    if (function.HasCallback)
                        builder.Append("__callback, ");

                    builder.Append('[');
                    {
                        for (int i = 0; i <= lastIndex; i++)
                            if (function.ParameterList[i].type is not null)
                                builder.AppendInterpolation($"{function.ParameterList[i].name}, ");

                        if (builder[^1] is ' ')
                            builder.Length -= 2;
                    }
                    builder.Append(']');

                    if (!isSync)
                        builder.Append(", cancellationToken");

                    builder.Append(");\n");
                }

                builder.Append("    }\n\n");
            } while (lastIndex >= 0 && function.ParameterList[lastIndex].optional);
        }
    }
}
