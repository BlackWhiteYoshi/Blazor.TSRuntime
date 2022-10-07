using Microsoft.CodeAnalysis;
using TSRuntime.Core.Generation;
using TSRuntime.FileWatching;

namespace TSRuntime.SourceGenerator;

[Generator]
public sealed class SourceGenerator : ISourceGenerator, IDisposable {
    private readonly FileWatcher fileWatcher = new("path/to/.csproj-folder/tsconfig.tsruntime.json");

    public void Dispose() => fileWatcher.Dispose();

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context) {
        context.AddSource(fileWatcher.Config.FileOutputClass, Generator.TSRuntimeContent);
        context.AddSource(fileWatcher.Config.FileOutputinterface, fileWatcher.Source);
    }
}
