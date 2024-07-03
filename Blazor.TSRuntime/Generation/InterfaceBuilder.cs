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
        private string scriptName;
        private TSFunction function;
        private readonly List<(MappedType parameter, MappedType[]? callback)> mappedParameterList = [];
        private MappedType mappedReturnType;
        private readonly List<GenericType> genericParameterList = [];
        private readonly List<GenericType> genericCallbackList = [];
        private string returnType;
        private string returnModifiers;


        /// <summary>
        /// Builds the interface for a module.
        /// </summary>
        /// <returns></returns>
        public (string hintName, string source) Build(TSModule module) {
            scriptName = module.Name;

            AppendUsingsAndNamespace();

            // head
            if (!config.ModuleGrouping)
                builder.Append("public partial interface ITSRuntime {\n");
            else {
                // summary
                builder.Append("/// <summary>\n");
                builder.Append("/// <para>Interface to JS-interop the module '");
                builder.Append(module.Name);
                builder.Append("'.</para>\n");
                builder.Append("/// <para>It contains invoke-methods for the js-functions and a preload-method for loading the module of '");
                builder.Append(module.Name);
                builder.Append("'.</para>\n");
                builder.Append("/// </summary>\n");

                builder.Append($"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"{AssemblyInfo.NAME}\", \"{AssemblyInfo.VERSION}\")]\n");

                builder.Append("public interface ");
                config.ModuleGroupingNamePattern.AppendNaming(builder, module.Name);
                builder.Append(" : ITSRuntime {\n");
            }

            // preload module
            {
                builder.Append("    protected Task<IJSObjectReference> Get");
                builder.Append(module.Name);
                builder.Append("Module();\n\n");

                builder.Append("    /// <summary>\n");
                builder.Append("    /// <para>Loads '");
                builder.Append(module.Name);
                builder.Append("' (");
                builder.Append(module.URLPath);
                builder.Append(") as javascript-module.</para>\n");
                builder.Append("    /// <para>If already loading, it does not trigger a second loading and if already loaded, it returns a completed task.</para>\n");
                builder.Append("    /// </summary>\n");
                builder.Append("    /// <returns>A Task that will complete when the module import have completed.</returns>\n");

                builder.Append("    public Task ");
                config.PreloadNamePattern.AppendNaming(builder, module.Name);
                builder.Append("() => Get");
                builder.Append(module.Name);
                builder.Append("Module();\n\n\n");
            }

            AppendFunctionList(module.FunctionList, isModule: true);

            builder.Length -= 2;
            builder.Append("}\n");


            string source = builder.ToString();

            builder.Clear();
            if (!config.ModuleGrouping) {
                builder.Append("ITSRuntime_");
                builder.Append(module.Name);
            }
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


            AppendUsingsAndNamespace();

            // head
            builder.Append("public partial interface ITSRuntime {\n");

            AppendFunctionList(script.FunctionList, isModule: false);

            if (builder[^2] is '\n')
                builder.Length -= 2;
            builder.Append("}\n");


            string source = builder.ToString();

            builder.Clear();
            builder.Append("ITSRuntime_");
            builder.Append(scriptName);
            builder.Append(".g.cs");
            string hintName = builder.ToString();


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
            foreach (string usingStatement in config.UsingStatements) {
                builder.Append("using ");
                builder.Append(usingStatement);
                builder.Append(";\n");
            }
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
        private void AppendFunctionList(IReadOnlyList<TSFunction> functionList, bool isModule) {
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
                        // attribute
                        for (int k = 0; k < function.ParameterList.Length; k++)
                            if (function.ParameterList[k].typeCallback.Length > 0) {
                                builder.Append("    [method: DynamicDependency(nameof(_");
                                builder.Append(function.ParameterList[k].name);
                                builder.Append("))]\n");
                            }
                        // class head beginning
                        builder.Append("    private sealed class ");
                        config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, string.Empty);
                        builder.Append("Callback");
                        // generics
                        if (genericCallbackList.Count > 0) {
                            builder.Append('<');
                            builder.Append(genericCallbackList[0].Name);
                            for (int k = 1; k < genericCallbackList.Count; k++) {
                                builder.Append(genericCallbackList[k].Name);
                                builder.Append(" ,");
                            }
                            builder.Append('>');
                        }
                        // generic constraints
                        foreach (GenericType genericType in genericCallbackList)
                            if (genericType.Constraint is not null) {
                                builder.Append(" where ");
                                builder.Append(genericType.Name);
                                builder.Append(" : ");
                                builder.Append(genericType.Constraint);
                            }
                        builder.Append(" {\n");
                        
                        for (int k = 0; k < function.ParameterList.Length; k++)
                            if (mappedParameterList[k].callback is MappedType[] callbackTypeList) {
                                builder.Append("        public ");
                                {
                                    if (function.ParameterList[k].typeCallback[^1].type is "void" && !function.ParameterList[k].typeCallbackPromise) {
                                        builder.Append("Action");
                                        if (callbackTypeList.Length > 1) {
                                            builder.Append('<');
                                            builder.Append(callbackTypeList[0].Type);
                                            for (int l = 1; l < callbackTypeList.Length - 1; l++) { // last parameter is returnType
                                                builder.Append(", ");
                                                builder.Append(callbackTypeList[l].Type);
                                            }
                                            builder.Append('>');
                                        }
                                    }
                                    else {
                                        builder.Append("Func<");
                                        for (int l = 0; l < callbackTypeList.Length - 1; l++) {
                                            builder.Append(callbackTypeList[l].Type);
                                            builder.Append(", ");
                                        }
                                        if (!function.ParameterList[k].typeCallbackPromise)
                                            builder.Append(callbackTypeList[^1].Type);
                                        else {
                                            builder.Append("ValueTask");
                                            if (function.ParameterList[k].typeCallback[^1].type is not "void") {
                                                builder.Append('<');
                                                builder.Append(callbackTypeList[^1].Type);
                                                builder.Append('>');
                                            }
                                        }
                                        builder.Append('>');
                                    }
                                }
                                builder.Append(" _");
                                builder.Append(function.ParameterList[k].name);
                                builder.Append(";\n");

                                builder.Append("        [JSInvokable] public ");

                                if (!function.ParameterList[k].typeCallbackPromise)
                                    builder.Append(callbackTypeList[^1].Type);
                                else {
                                    builder.Append("ValueTask");
                                    if (function.ParameterList[k].typeCallback[^1].type is not "void") {
                                        builder.Append('<');
                                        builder.Append(callbackTypeList[^1].Type);
                                        builder.Append('>');
                                    }
                                }
                                
                                builder.Append(' ');
                                builder.Append(function.ParameterList[k].name);
                                builder.Append('(');
                                for (int l = 0; l < callbackTypeList.Length - 1; l++) { // last parameter is returnType
                                    builder.Append(callbackTypeList[l].Type);
                                    builder.Append(' ');
                                    builder.Append(function.ParameterList[k].typeCallback[l].name);
                                    builder.Append(", ");
                                }
                                if (builder[^1] is ' ')
                                    builder.Length -= 2;
                                builder.Append(") => _");
                                builder.Append(function.ParameterList[k].name);
                                builder.Append('(');
                                for (int l = 0; l < function.ParameterList[k].typeCallback.Length - 1; l++) { // last parameter is returnType
                                    builder.Append(function.ParameterList[k].typeCallback[l].name);
                                    builder.Append(", ");
                                }
                                if (builder[^1] is ' ')
                                    builder.Length -= 2;
                                builder.Append(");\n\n");
                            }
                        builder.Length--;
                        builder.Append("    }\n");
                    }

                    if (function.ReturnPromise && config.PromiseOnlyAsync)
                        AppendInvokeAsyncMethod("asynchronously.", config.InvokeFunctionActionNameAsync, "TSInvokeAsync", isModule);
                    else {
                        if (config.InvokeFunctionSyncEnabled)
                            AppendInvokeSyncMethod("synchronously.", config.InvokeFunctionActionNameSync, "TSInvoke", isModule);
                        if (config.InvokeFunctionTrySyncEnabled)
                            AppendInvokeAsyncMethod("synchronously when supported, otherwise asynchronously.", config.InvokeFunctionActionNameTrySync, "TSInvokeTrySync", isModule);
                        if (config.InvokeFunctionAsyncEnabled)
                            AppendInvokeAsyncMethod("asynchronously.", config.InvokeFunctionActionNameAsync, "TSInvokeAsync", isModule);
                    }
                }
                builder.Append('\n');
            }
        }


        /// <summary>
        /// <para>Appends the invoke method for a js function.</para>
        /// <para>
        /// If the parameterlist of the js function ends on optional variables, corresponding overload methods are created.<br />
        /// In this case multiple methods for that js function are appended.
        /// </para>
        /// <para>This method builds for sync Invoke calls only.</para>
        /// </summary>
        /// <param name="summaryAction"></param>
        /// <param name="invokeFunctionActionName"></param>
        /// <param name="invokeFunction"></param>
        /// <param name="isModule"></param>
        private readonly void AppendInvokeSyncMethod(string summaryAction, string invokeFunctionActionName, string invokeFunction, bool isModule)
            => AppendInvokeMethodCore(summaryAction, invokeFunctionActionName, invokeFunction, isModule, isSync: true);

        /// <summary>
        /// <para>Appends the invoke method for a js function.</para>
        /// <para>
        /// If the parameterlist of the js function ends on optional variables, corresponding overload methods are created.<br />
        /// In this case multiple methods for that js function are appended.
        /// </para>
        /// <para>This method builds for async Invoke calls (TrySync, Async).</para>
        /// </summary>
        /// <param name="summaryAction"></param>
        /// <param name="invokeFunctionActionName"></param>
        /// <param name="invokeFunction"></param>
        /// <param name="isModule"></param>
        private readonly void AppendInvokeAsyncMethod(string summaryAction, string invokeFunctionActionName, string invokeFunction, bool isModule)
            => AppendInvokeMethodCore(summaryAction, invokeFunctionActionName, invokeFunction, isModule, isSync: false);

        /// <summary>
        /// Core method for <see cref="AppendInvokeSyncMethod"/> and <see cref="AppendInvokeAsyncMethod"/>.
        /// </summary>
        /// <param name="summaryAction"></param>
        /// <param name="invokeFunctionActionName"></param>
        /// <param name="invokeFunction"></param>
        /// <param name="isModule"></param>
        /// <param name="isSync"></param>
        private readonly void AppendInvokeMethodCore(string summaryAction, string invokeFunctionActionName, string invokeFunction, bool isModule, bool isSync) {
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
                builder.Append("    /// <summary>\n");
                if (function.Summary != string.Empty) {
                    builder.Append("    /// <para>");
                    builder.Append(function.Summary);
                    builder.Append("</para>\n");
                }
                if (isModule) {
                    builder.Append("    /// <para>Invokes in module '");
                    builder.Append(scriptName);
                    builder.Append("' the JS-function '");
                    builder.Append(function.Name);
                    builder.Append("' ");
                    builder.Append(summaryAction);
                    builder.Append("</para>\n");
                    if (isSync)
                        builder.Append("    /// <para>If module is not loaded or synchronous is not supported, it fails with an exception.</para>\n");
                }
                else {
                    builder.Append("    /// <para>Invokes in script '");
                    builder.Append(scriptName);
                    builder.Append("' the JS-function '");
                    builder.Append(function.Name);
                    builder.Append("' ");
                    builder.Append(summaryAction);
                    builder.Append("</para>\n");
                }
                builder.Append("    /// </summary>\n");
                // <remarks>
                if (function.Remarks != string.Empty) {
                    builder.Append("    /// <remarks>");
                    builder.Append(function.Remarks);
                    builder.Append("</remarks>\n");
                }
                // <typeparam>
                foreach (GenericType genericType in genericParameterList) {
                    builder.Append("    /// <typeparam name=\"");
                    builder.Append(genericType.Name);
                    builder.Append("\"></typeparam>\n");
                }
                foreach ((string type, string description) in function.Generics) {
                    builder.Append("    /// <typeparam name=\"");
                    builder.Append(type);
                    builder.Append("\">");
                    builder.Append(description);
                    builder.Append("</typeparam>\n");
                }
                // <param>
                for (int i = 0; i <= lastIndex; i++) {
                    builder.Append("    /// <param name=\"");
                    builder.Append(function.ParameterList[i].name);
                    builder.Append("\">");
                    builder.Append(function.ParameterList[i].summary);
                    builder.Append("</param>\n");
                }
                if (!isSync)
                    builder.Append("    /// <param name=\"cancellationToken\">A cancellation token to signal the cancellation of the operation. Specifying this parameter will override any default cancellations such as due to timeouts (<see cref=\"JSRuntime.DefaultAsyncTimeout\"/>) from being applied.</param>\n");
                // <returns>
                if (function.ReturnType.summary != string.Empty) {
                    builder.Append("    /// <returns>");
                    builder.Append(function.ReturnType.summary);
                    builder.Append("</returns>\n");
                }
                else if (returnType != "void")
                    builder.Append("    /// <returns>Result of the JS-function.</returns>\n");
                else if (!isSync)
                    builder.Append("    /// <returns>A Task that will complete when the JS-Function have completed.</returns>\n");

                // method head (visibility and returnType)
                if (isSync) {
                    builder.Append("    public ");
                    builder.Append(returnType);
                    builder.Append(returnModifiers);
                    builder.Append(' ');
                }
                else {
                    builder.Append("    public async ValueTask");
                    if (returnType is not "void") {
                        builder.Append('<');
                        builder.Append(returnType);
                        builder.Append(returnModifiers);
                        builder.Append('>');
                    }
                    builder.Append(' ');
                }

                // method name
                config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, invokeFunctionActionName);
                if (function.ReturnPromise && config.PromiseAppendAsync)
                    builder.Append("Async");

                // generic parameters
                if (genericParameterList.Count > 0 || function.Generics.Length > 0) {
                    builder.Append('<');
                    for (int i = 0; i < genericParameterList.Count; i++) {
                        builder.Append(genericParameterList[i].Name);
                        builder.Append(", ");
                    }
                    foreach ((string genericType, _) in function.Generics) {
                        builder.Append(genericType);
                        builder.Append(", ");
                    }
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
                                    for (int j = 1; j < callbackTypeList.Length - 1; j++) { // last parameter is returnType
                                        builder.Append(", ");
                                        builder.Append(callbackTypeList[j].Type);
                                    }
                                    builder.Append('>');
                                }
                            }
                            else {
                                builder.Append("Func<");
                                for (int j = 0; j < callbackTypeList.Length - 1; j++) {
                                    builder.Append(callbackTypeList[j].Type);
                                    builder.Append(", ");
                                }
                                if (!function.ParameterList[i].typeCallbackPromise)
                                    builder.Append(callbackTypeList[^1].Type);
                                else {
                                    builder.Append("ValueTask");
                                    if (function.ParameterList[i].typeCallback[^1].type is not "void") {
                                        builder.Append('<');
                                        builder.Append(callbackTypeList[^1].Type);
                                        builder.Append('>');
                                    }
                                }
                                builder.Append('>');
                            }
                        }
                        if (function.ParameterList[i].typeNullable)
                            builder.Append('?');
                        if (function.ParameterList[i].array)
                            builder.Append("[]");
                        if (function.ParameterList[i].arrayNullable)
                            builder.Append('?');
                        builder.Append(' ');
                        builder.Append(function.ParameterList[i].name);
                        builder.Append(", ");
                    }
                    if (isSync)
                        builder.Length -= 2;
                }
                if (!isSync)
                    builder.Append("CancellationToken cancellationToken = default");
                builder.Append(')');

                // generic constraints
                foreach (GenericType genericType in genericParameterList)
                    if (genericType.Constraint is not null) {
                        builder.Append(" where ");
                        builder.Append(genericType.Name);
                        builder.Append(" : ");
                        builder.Append(genericType.Constraint);
                    }

                builder.Append(" {\n");


                // body
                if (function.HasCallback) {
                    builder.Append("        using DotNetObjectReference<");
                    config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, string.Empty);
                    builder.Append("Callback");
                    if (genericCallbackList.Count > 0) {
                        builder.Append('<');
                        builder.Append(genericCallbackList[0].name);
                        for (int i = 1; i < genericCallbackList.Count; i++) {
                            builder.Append(", ");
                            builder.Append(genericCallbackList[i].name);
                        }
                        builder.Append('>');
                    }
                    builder.Append("> __callback");
                    builder.Append(" = DotNetObjectReference.Create(new ");
                    config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, string.Empty);
                    builder.Append("Callback");
                    if (genericCallbackList.Count > 0) {
                        builder.Append('<');
                        builder.Append(genericCallbackList[0].name);
                        for (int i = 1; i < genericCallbackList.Count; i++) {
                            builder.Append(", ");
                            builder.Append(genericCallbackList[i].name);
                        }
                        builder.Append('>');
                    }
                    builder.Append("() { ");
                    for (int i = 0; i < function.ParameterList.Length; i++)
                        if (function.ParameterList[i].typeCallback.Length > 0) {
                            builder.Append('_');
                            builder.Append(function.ParameterList[i].name);
                            builder.Append(" = ");
                            builder.Append(function.ParameterList[i].name);
                            builder.Append(", ");
                        }
                    builder.Length -= 2;
                    builder.Append(" });\n");
                }

                builder.Append("        ");
                if (returnType is not "void")
                    builder.Append("return ");
                if (!isSync)
                    builder.Append("await ");
                builder.Append(invokeFunction);
                builder.Append('<');
                if (returnType is "void")
                    builder.Append("Infrastructure.IJSVoidResult");
                else {
                    builder.Append(returnType);
                    builder.Append(returnModifiers);
                }
                if (function.HasCallback) {
                    builder.Append(", ");
                    config.InvokeFunctionNamePattern.AppendNaming(builder, scriptName, function.Name, string.Empty);
                    builder.Append("Callback");
                    if (genericCallbackList.Count > 0) {
                        builder.Append('<');
                        builder.Append(genericCallbackList[0].name);
                        for (int i = 1; i < genericCallbackList.Count; i++) {
                            builder.Append(", ");
                            builder.Append(genericCallbackList[i].name);
                        }
                        builder.Append('>');
                    }
                }
                builder.Append(">(");
                if (isModule) {
                    builder.Append("Get");
                    builder.Append(scriptName);
                    builder.Append("Module(), ");
                }
                builder.Append('"');
                builder.Append(function.Name);
                builder.Append("\", ");
                if (function.HasCallback)
                    builder.Append("__callback, ");
                builder.Append('[');
                if (lastIndex >= 0) {
                    for (int i = 0; i <= lastIndex; i++) {
                        if (function.ParameterList[i].type is not null) {
                            builder.Append(function.ParameterList[i].name);
                            builder.Append(", ");
                        }
                    }
                    if (builder[^1] is ' ')
                        builder.Length -= 2;
                }
                builder.Append(']');
                if (!isSync)
                    builder.Append(", cancellationToken");
                builder.Append(");\n");
                builder.Append("    }\n\n");
            } while (lastIndex >= 0 && function.ParameterList[lastIndex].optional);
        }
    }
}
