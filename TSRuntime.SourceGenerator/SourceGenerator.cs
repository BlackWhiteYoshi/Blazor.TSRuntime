using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.ObjectPool;
using System.Text;
using TSRuntime.Core.Configs;
using TSRuntime.Core.Generation;
using TSRuntime.Core.Parsing;
using TSRuntime.FileWatching;

namespace TSRuntime.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class SourceGenerator : ISourceGenerator, IDisposable {
    private TSFileWatcher? fileWatcher;

    private string source = string.Empty;
    private readonly ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool(initialCapacity: 8 * 1024, maximumRetainedCapacity: 1024 * 1024);

    private void CreateITSRuntimeContentString(TSStructureTree structureTree, Config config) {
        StringBuilder sourceBuilder = stringBuilderPool.Get();

        foreach (string str in Generator.GetITSRuntimeContent(structureTree, config))
            sourceBuilder.Append(str);
        source = sourceBuilder.ToString();

        stringBuilderPool.Return(sourceBuilder);
    }


    public void Dispose() {
        Destructor();
        GC.SuppressFinalize(this);
    }

    ~SourceGenerator() {
        Destructor();
    }

    private void Destructor() {
        if (fileWatcher != null) {
            fileWatcher.StructureTreeChanged -= CreateITSRuntimeContentString;
            fileWatcher.Dispose();
        }
    }


    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context) {
        if (fileWatcher == null) {
            AdditionalText? file = context.AdditionalFiles.FirstOrDefault((AdditionalText file) => Path.GetFileName(file.Path) == TSFileWatcher.JSON_FILE_NAME);
            if (file == null)
                return;

            SourceText? jsonSourceText = file.GetText(context.CancellationToken);
            if (jsonSourceText == null)
                return;

            Config config = new(jsonSourceText.ToString());
            string basePath = Path.GetDirectoryName(file.Path).Replace('\\', '/');

            // first time could be just one time compiling, so no need to instantiate fileWatcher
            if (source == string.Empty) {
                DeclarationPath[] declarationPath = TSFileWatcher.ConvertToAbsolutePath(config.DeclarationPath, basePath);
                TSStructureTree structureTree = TSStructureTree.ParseFiles(declarationPath).GetAwaiter().GetResult();
                CreateITSRuntimeContentString(structureTree, config);
            }
            else {
                fileWatcher = new TSFileWatcher(config, basePath);
                fileWatcher.StructureTreeChanged += CreateITSRuntimeContentString;
                _ = fileWatcher.CreateModuleWatcher();
            }
        }

        context.AddSource("TSRuntime.g.cs", Generator.TSRuntimeContent);
        context.AddSource("ITSRuntime.g.cs", source);
    }
}
