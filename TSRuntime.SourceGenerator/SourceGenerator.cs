using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using TSRuntime.Core.Configs;
using TSRuntime.Core.Generation;
using TSRuntime.Core.Parsing;
using TSRuntime.FileWatching;

namespace TSRuntime.SourceGenerator;

[Generator]
public sealed class SourceGenerator : ISourceGenerator, IDisposable {
    private TSFileWatcher? fileWatcher;

    private string source = string.Empty;
    private readonly StringBuilder sourceBuilder = new(10000);

    private void CreateITSRuntimeContentString(TSStructureTree structureTree, Config config) {
        lock (sourceBuilder) {
            sourceBuilder.Clear();
            foreach (string str in Generator.GetITSRuntimeContent(structureTree, config))
                sourceBuilder.Append(str);
        
            source = sourceBuilder.ToString();
        }
    }


    public void Dispose() {
        if (fileWatcher != null) {
            fileWatcher.StructureTreeChanged -= CreateITSRuntimeContentString;
            fileWatcher.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    ~SourceGenerator() {
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
                TSStructureTree structureTree = new();
                structureTree.ParseModules(declarationPath).GetAwaiter().GetResult();
                CreateITSRuntimeContentString(structureTree, config);
            }
            else
                TSFileWatcher.CreateTSFileWatcher(config, basePath, CreateITSRuntimeContentString)
                    .ContinueWith((Task<TSFileWatcher> fileWatcherTask) => fileWatcher = fileWatcherTask.Result);
        }

        context.AddSource("TSRuntime.g.cs", Generator.TSRuntimeContent);
        context.AddSource("ITSRuntime.g.cs", source);
    }
}
