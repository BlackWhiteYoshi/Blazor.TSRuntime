using System.Text;
using TSRuntime.Core.Configs;
using TSRuntime.Core.Generation;
using TSRuntime.Core.Parsing;

namespace TSRuntime.FileWatching;

public sealed class FileWatcher : IDisposable {
    private readonly FileSystemWatcher watcher = new();

    public Config Config { get; private set; }
    public string Source { get; private set; }

    private readonly TSSyntaxTree syntaxTree = new();
    private readonly Dictionary<string, int> moduleMap = new();
    


    public FileWatcher(string configPath) {
        string json = File.ReadAllText(configPath);
        Config = Config.FromJson(json);

        syntaxTree.ParseModules(Config.DeclarationPath);
        for (int i = 0; i < syntaxTree.ModuleList.Count; i++)
            moduleMap.Add(syntaxTree.ModuleList[i].FilePath, i);
        Source = ITSRuntimeContentToString();


        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;

        watcher.Filter = "*.d.ts";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
    }

    public void Dispose() => watcher.Dispose();


    #region FileSystemWatcher methods

    private void OnChanged(object sender, FileSystemEventArgs e) {
        if (e.ChangeType != WatcherChangeTypes.Changed)
            return;

        if (moduleMap.TryGetValue(e.FullPath, out int index)) {
            syntaxTree.ModuleList[index].ParseFunctions();
            Source = ITSRuntimeContentToString();
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e) {
        TSModule module = TSModule.Parse(e.FullPath, Config.DeclarationPath);
        moduleMap.Add(module.FilePath, syntaxTree.ModuleList.Count);
        syntaxTree.ModuleList.Add(module);

        Source = ITSRuntimeContentToString();
    }

    private void OnDeleted(object sender, FileSystemEventArgs e) {
        if (moduleMap.TryGetValue(e.FullPath, out int index)) {
            moduleMap.Remove(e.FullPath);
            syntaxTree.ModuleList.RemoveAt(index);
        }

        Source = ITSRuntimeContentToString();
    }

    private void OnRenamed(object sender, RenamedEventArgs e) {
        if (moduleMap.TryGetValue(e.OldFullPath, out int index)) {
            moduleMap.Remove(e.OldFullPath);
            moduleMap.Add(e.FullPath, index);
            syntaxTree.ModuleList[index].ParseMetaData(e.FullPath, Config.DeclarationPath);
        }

        Source = ITSRuntimeContentToString();
    }

    #endregion


    #region ITSRuntimeContent to string

    private readonly StringBuilder sourceBuilder = new(10000);

    private string ITSRuntimeContentToString() {
        sourceBuilder.Clear();

        foreach (string str in Generator.GetITSRuntimeContent(syntaxTree, Config))
            sourceBuilder.Append(str);

        return sourceBuilder.ToString();
    }

    #endregion
}
