using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using TSRuntime.Configs;
using TSRuntime.Generation;
using TSRuntime.Parsing;
using ConfigOrError = (TSRuntime.Configs.Config? config, Microsoft.CodeAnalysis.Diagnostic? error);

namespace TSRuntime;

[Generator(LanguageNames.CSharp)]
public sealed class TSRuntimeGenerator : IIncrementalGenerator {
    private readonly ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool(initialCapacity: 8192, maximumRetainedCapacity: 1024 * 1024);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValueProvider<ConfigOrError> configProvider = context.AdditionalTextsProvider
            .Where((AdditionalText textFile) => textFile.Path.EndsWith("tsruntime.json"))
            .Collect()
            .Select((ImmutableArray<AdditionalText> textFiles, CancellationToken cancellationToken) => {
                if (textFiles.Length == 0)
                    return (string.Empty, string.Empty, DiagnosticErrors.CreateNoConfigFileError());
                if (textFiles.Length >= 2)
                    return (string.Empty, string.Empty, DiagnosticErrors.CreateMultipleConfigFilesError());

                AdditionalText textFile = textFiles[0];
                string configPath = Path.GetDirectoryName(textFile.Path);

                SourceText? configContent = textFile.GetText(cancellationToken);
                if (configContent is null)
                    return (string.Empty, string.Empty, DiagnosticErrors.CreateFileReadingError(textFile.Path));

                return (configPath, configContent.ToString(), (Diagnostic?)null);
            })
            .Select(((string path, string content, Diagnostic? error) file, CancellationToken cancellationToken) => file.error switch {
                null => (new Config(file.path, file.content), (Diagnostic?)null),
                _ => ((Config?)null, file.error)
            });


        IncrementalValuesProvider<(TSFile? file, string content, Config? config)> fileList = context.AdditionalTextsProvider
            .Combine(configProvider)
            .Select<(AdditionalText, ConfigOrError), (TSFile?, string, Config?)>(((AdditionalText textFile, ConfigOrError configOrError) parameters, CancellationToken cancellationToken) => {
                if (parameters.configOrError.error is not null)
                    return (null, string.Empty, null);

                Config config = parameters.configOrError.config!;
                AdditionalText textFile = parameters.textFile;

                string modulePath = textFile.Path.Replace('\\', '/');
                if (!modulePath.StartsWith(config.WebRootPath))
                    return (null, string.Empty, config);
                modulePath = modulePath[config.WebRootPath.Length..];

                foreach (InputPath inputPath in config.InputPath)
                    if (inputPath.IsIncluded(modulePath)) {
                        if (inputPath.ModuleFiles) {
                            TSModule module = new(modulePath, inputPath.ModulePath, config.ErrorList);

                            SourceText? content = textFile.GetText(cancellationToken);
                            if (content is null) {
                                Diagnostic error = DiagnosticErrors.CreateFileReadingError(textFile.Path);
                                return (module, string.Empty, config);
                            }

                            return (module, content.ToString(), config);
                        }
                        else {
                            TSScript script = new(modulePath, config.ErrorList);

                            SourceText? content = textFile.GetText(cancellationToken);
                            if (content is null) {
                                Diagnostic error = DiagnosticErrors.CreateFileReadingError(textFile.Path);
                                return (script, string.Empty, config);
                            }

                            return (script, content.ToString(), config);
                        }
                    }

                return (null, string.Empty, config);
            });

        IncrementalValuesProvider<(TSModule, Config)> moduleList = fileList
            .Where(((TSFile? file, string content, Config? config) source) => source.file is TSModule)
            .Select(((TSFile? file, string content, Config? config) source, CancellationToken _) => {
                Debug.Assert(source.file is TSModule && source.config is not null);
                TSModule module = (TSModule)source.file!;
                Config config = source.config!;

                TSModule moduleWithFunctionsParsed = new(module.FilePath, module.URLPath, module.Name, TSFunction.ParseFile(source.content, isModule: true, config, module.FilePath));
                return (moduleWithFunctionsParsed, config);
            });

        IncrementalValuesProvider<(TSScript, Config)> scriptList = fileList
            .Where(((TSFile? file, string content, Config? config) source) => source.file is TSScript)
            .Select(((TSFile? file, string content, Config? config) source, CancellationToken _) => {
                Debug.Assert(source.file is TSScript && source.config is not null);
                TSScript script = (TSScript)source.file!;
                Config config = source.config!;

                TSScript scriptWithFunctionsParsed = new(script.FilePath, script.URLPath, script.Name, TSFunction.ParseFile(source.content, isModule: false, config, script.FilePath));
                return (scriptWithFunctionsParsed, config);
            });


        IncrementalValueProvider<(ImmutableArray<TSModule> moduleList, ConfigOrError configOrError)> moduleCollectionWithConfig = moduleList
            .Select(((TSModule module, Config config) tuple, CancellationToken _) => tuple.module)
            .Collect()
            .Combine(configProvider);

        IncrementalValueProvider<(ImmutableArray<TSScript> scriptList, (ImmutableArray<TSModule> moduleList, ConfigOrError configOrError) tuple)> scriptModuleCollectionWithConfig = scriptList
            .Select(((TSScript script, Config config) tuple, CancellationToken _) => tuple.script)
            .Collect()
            .Combine(moduleCollectionWithConfig);


        context.RegisterSourceOutput(scriptModuleCollectionWithConfig, stringBuilderPool.BuildClass);

        context.RegisterSourceOutput(configProvider, InterfaceCoreBuilder.BuildInterfaceCore);
        context.RegisterSourceOutput(moduleList, stringBuilderPool.BuildInterfaceModule);
        context.RegisterSourceOutput(scriptList, stringBuilderPool.BuildInterfaceScript);

        context.RegisterSourceOutput(moduleCollectionWithConfig, stringBuilderPool.BuildServiceExtension);
    }
}
