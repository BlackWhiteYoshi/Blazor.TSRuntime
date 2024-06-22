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
        private readonly List<GenericType> genericParameterList = [];
        private readonly List<MappedType> mappedParameterList = [];
        private MappedType mappedReturnType;
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
                mappedParameterList.Clear();
                genericParameterList.Clear();
                for (int i = 0; i < functionList.Count; i++) {
                    function = functionList[i];

                    mappedParameterList.Clear();
                    for (int j = 0; j < function.ParameterList.Length; j++)
                        if (config.TypeMap.TryGetValue(function.ParameterList[j].type, out MappedType mappedType))
                            mappedParameterList.Add(mappedType);
                        else
                            mappedParameterList.Add(new MappedType(function.ParameterList[j].type));

                    if (!config.TypeMap.TryGetValue(function.ReturnType.type, out mappedReturnType))
                        mappedReturnType = new(function.ReturnType.type);

                    returnType = mappedReturnType.Type;
                    returnModifiers = (function.ReturnType.typeNullable, function.ReturnType.array, function.ReturnType.arrayNullable) switch {
                        (false, false, _) => string.Empty,
                        (true, false, _) => "?",
                        (false, true, false) => "[]",
                        (false, true, true) => "[]?",
                        (true, true, false) => "?[]",
                        (true, true, true) => "?[]?"
                    };

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
                for (int i = 0; i <= lastIndex; i++)
                    for (int j = 0; j < mappedParameterList[i].GenericTypes.Length; j++)
                        if (!genericParameterList.Contains(mappedParameterList[i].GenericTypes[j]))
                            genericParameterList.Add(mappedParameterList[i].GenericTypes[j]);
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
                    builder.Append("    public ");
                    if (returnType == "void")
                        builder.Append("Task ");
                    else {
                        builder.Append("ValueTask<");
                        builder.Append(returnType);
                        builder.Append(returnModifiers);
                        builder.Append("> ");
                    }
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
                    foreach (string genericType in function.Generics) {
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
                        builder.Append(mappedParameterList[i].Type);
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
                    if (genericType.Constraint != null) {
                        builder.Append(" where ");
                        builder.Append(genericType.Name);
                        builder.Append(" : ");
                        builder.Append(genericType.Constraint);
                    }

                builder.Append("\n        => ");
                builder.Append(invokeFunction);
                builder.Append('<');
                if (returnType == "void")
                    builder.Append("Infrastructure.IJSVoidResult");
                else {
                    builder.Append(returnType);
                    builder.Append(returnModifiers);
                }
                builder.Append(">(");
                if (isModule) {
                    builder.Append("Get");
                    builder.Append(scriptName);
                    builder.Append("Module(), ");
                }
                builder.Append('"');
                builder.Append(function.Name);
                builder.Append("\", [");
                if (lastIndex >= 0) {
                    for (int i = 0; i < lastIndex; i++) {
                        builder.Append(function.ParameterList[i].name);
                        builder.Append(", ");
                    }
                    builder.Append(function.ParameterList[lastIndex].name);
                }
                builder.Append(']');
                if (!isSync)
                    builder.Append(", cancellationToken");
                builder.Append(')');

                if (!isSync)
                    if (returnType == "void") {
                        builder.Append(" switch {\n");
                        builder.Append("            ValueTask<");
                        if (returnType == "void")
                            builder.Append("Infrastructure.IJSVoidResult");
                        else {
                            builder.Append(returnType);
                            builder.Append(returnModifiers);
                        }
                        builder.Append("> { IsCompleted: true } => Task.CompletedTask,\n");
                        builder.Append("            ValueTask<");
                        if (returnType == "void")
                            builder.Append("Infrastructure.IJSVoidResult");
                        else {
                            builder.Append(returnType);
                            builder.Append(returnModifiers);
                        }
                        builder.Append("> task => task.AsTask()\n");
                        builder.Append("        }");
                    }

                builder.Append(";\n\n");
            } while (lastIndex >= 0 && function.ParameterList[lastIndex].optional);
        }
    }
}
