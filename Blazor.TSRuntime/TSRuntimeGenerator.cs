using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Immutable;
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
            .Select(((string path, string content, Diagnostic? error) file, CancellationToken cancellationToken) => file.error is null ? (new Config(file.path, file.content), (Diagnostic?)null) : ((Config?)null, file.error));

        IncrementalValuesProvider<(TSModule, Config)> moduleList = context.AdditionalTextsProvider
            .Combine(configProvider)
            .Select<(AdditionalText, ConfigOrError), (TSModule?, string, Config?)> (((AdditionalText textFile, ConfigOrError configOrError) parameters, CancellationToken cancellationToken) => {
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
                        TSModule module = new(modulePath, inputPath.ModulePath, config.ErrorList);

                        SourceText? content = textFile.GetText(cancellationToken);
                        if (content is null) {
                            Diagnostic error = DiagnosticErrors.CreateFileReadingError(textFile.Path);
                            return (module, string.Empty, config);
                        }

                        return (module, content.ToString(), config);
                    }

                return (null, string.Empty, config);
            })
            .Where(((TSModule? module, string content, Config? config) source) => source.module is not null)!
            .Select(((TSModule module, string content, Config config) source, CancellationToken _) => (source.module.ParseFunctions(source.content, source.config), source.config));

        IncrementalValueProvider<(ImmutableArray<TSModule> moduleList, ConfigOrError configOrError)> moduleCollectionWithConfig = moduleList
            .Select(((TSModule module, Config config) tuple, CancellationToken _) => tuple.module)
            .Collect()
            .Combine(configProvider);


        context.RegisterSourceOutput(moduleCollectionWithConfig, stringBuilderPool.BuildClass);

        context.RegisterSourceOutput(configProvider, Builder.BuildInterfaceCore);
        context.RegisterSourceOutput(moduleList, stringBuilderPool.BuildInterfaceModule);

        context.RegisterSourceOutput(moduleCollectionWithConfig, stringBuilderPool.BuildServiceExtension);
    }
}
