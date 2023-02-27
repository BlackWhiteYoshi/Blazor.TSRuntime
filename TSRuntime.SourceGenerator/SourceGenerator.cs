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

    private void CreateITSRuntimeContentString(TSStructureTree structureTree) {
        if (fileWatcher == null)
            return;

        sourceBuilder.Clear();
        foreach (string str in Generator.GetITSRuntimeContent(structureTree, fileWatcher.Config))
            sourceBuilder.Append(str);
        source = sourceBuilder.ToString();
    }


    public void Dispose() {
        fileWatcher?.Dispose();
        GC.SuppressFinalize(this);
    }

    ~SourceGenerator() => fileWatcher?.Dispose();


    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context) {
        if (fileWatcher == null) {
            AdditionalText? file = context.AdditionalFiles.FirstOrDefault((AdditionalText file) => Path.GetFileName(file.Path) == Config.JSON_FILE_NAME);
            if (file == null)
                return;

            SourceText? jsonSourceText = file.GetText(context.CancellationToken);
            if (jsonSourceText == null)
                return;

            Config config = Config.FromJson(jsonSourceText.ToString());

            fileWatcher = new TSFileWatcher(config, Path.GetDirectoryName(file.Path));
            fileWatcher.ITSRuntimeChanged += CreateITSRuntimeContentString;
            fileWatcher.CreateStructureTree().GetAwaiter().GetResult();
        }

        context.AddSource(fileWatcher.Config.FileOutputClass, Generator.TSRuntimeContent);
        context.AddSource(fileWatcher.Config.FileOutputinterface, source);
    }
}
