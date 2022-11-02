using TSRuntime.Core.Configs;
using TSRuntime.Core.Parsing;

namespace TSRuntime.FileWatching;

public sealed class TSFileWatcher : IDisposable {
    private readonly FileSystemWatcher condigWatcher;
    private readonly FileSystemWatcher moduleWatcher;

    private readonly TSSyntaxTree syntaxTree = new();
    private readonly Dictionary<string, int> moduleMap = new();

    private readonly object _lock = new();


    public Config Config { get; private set; }

    public event Action? TSRuntimeLocationChanged;
    public event Action? ITSRuntimeLocationChanged;
    public event Action<TSSyntaxTree>? ITSRuntimeChanged;


    /// <summary>
    /// <para>Watches module folder specified in <see cref="Config.DeclarationPath"/> and the config file tsconfig.tsruntime.json.</para>
    /// <para>When a change is detected, <see cref="syntaxTree"/> / <see cref="Config"/> is updated accordingly.</para>
    /// </summary>
    /// <param name="config">If null, default config is supplied.</param>
    /// <param name="basePath">Directory where tsconfig.tsruntime.json file is located.<br />It's also the starting point for relative pathes.</param>
    public TSFileWatcher(Config? config, string basePath) {
        if (config != null)
            Config = config;
        else
            Config = new();

        condigWatcher = CreateConfigWatcher(basePath);
        moduleWatcher = CreateModuleWatcher(Path.Combine(basePath, Config.DeclarationPath));


        _ = ParseSyntaxTreeAsync(this);
    }

    public void Dispose() {
        condigWatcher.Dispose();
        moduleWatcher.Dispose();
    }


    private static async Task ParseSyntaxTreeAsync(TSFileWatcher me) {
        TSSyntaxTree localSyntaxTree = new();
        await localSyntaxTree.ParseModules(me.Config.DeclarationPath);

        lock (me._lock) {
            me.syntaxTree.ModuleList.AddRange(localSyntaxTree.ModuleList);
            for (int i = 0; i < me.syntaxTree.ModuleList.Count; i++)
                me.moduleMap.Add(localSyntaxTree.ModuleList[i].FilePath, i);

            me.ITSRuntimeChanged?.Invoke(me.syntaxTree);
        }
    }


    #region config watcher

    private FileSystemWatcher CreateConfigWatcher(string path) {
        FileSystemWatcher watcher = new(path, Config.JSON_FILE_NAME);

        watcher.Changed += OnConfigChanged;
        watcher.Created += OnConfigCreated;
        watcher.Deleted += OnConfigDeleted;
        watcher.Renamed += OnConfigRenamed;

        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        return watcher;
    }

    private void OnConfigChanged(object sender, FileSystemEventArgs e) => _ = UpdateConfig(e.FullPath);

    private void OnConfigCreated(object sender, FileSystemEventArgs e) => _ = UpdateConfig(e.FullPath);

    private void OnConfigDeleted(object sender, FileSystemEventArgs e) => UpdateConfig(new Config());

    private void OnConfigRenamed(object sender, RenamedEventArgs e) {
        if (e.Name == Config.JSON_FILE_NAME)
            _ = UpdateConfig(e.FullPath);
        else
            UpdateConfig(new Config());
    }


    private async Task UpdateConfig(string jsonPath) {
        try {
            using StreamReader streamReader = new(jsonPath);
            string json = await streamReader.ReadToEndAsync();
            UpdateConfig(Config.FromJson(json));
        }
        catch (IOException) { }
    }
    
    private void UpdateConfig(Config config) {
        Config oldConfig = Config;
        Config = config;


        if (Config.FileOutputClass != oldConfig.FileOutputClass)
            TSRuntimeLocationChanged?.Invoke();

        if (Config.FileOutputinterface != oldConfig.FileOutputinterface) 
            ITSRuntimeLocationChanged?.Invoke();

        if (Config.DeclarationPath != oldConfig.DeclarationPath) {
            syntaxTree.ModuleList.Clear();
            moduleMap.Clear();
            _ = ParseSyntaxTreeAsync(this);
            return;
        }
        
        if (Config.ModuleInvokeEnabled != oldConfig.ModuleInvokeEnabled)
            { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

        if (Config.ModuleTrySyncEnabled != oldConfig.ModuleTrySyncEnabled)
            { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

        if (Config.ModuleAsyncEnabled != oldConfig.ModuleAsyncEnabled)
            { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

        if (Config.JSRuntimeInvokeEnabled != oldConfig.JSRuntimeInvokeEnabled)
            { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

        if (Config.JSRuntimeTrySyncEnabled != oldConfig.JSRuntimeTrySyncEnabled)
            { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

        if (Config.JSRuntimeAsyncEnabled != oldConfig.JSRuntimeAsyncEnabled)
            { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

        if (Config.FunctionNamePattern != oldConfig.FunctionNamePattern)
            { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

        if (Config.UsingStatements.Length != oldConfig.UsingStatements.Length)
            { ITSRuntimeChanged?.Invoke(syntaxTree); return; }
        for (int i = 0; i < Config.UsingStatements.Length; i++)
            if (Config.UsingStatements[i] != oldConfig.UsingStatements[i])
                { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

        if (Config.TypeMap.Count != oldConfig.TypeMap.Count) { ITSRuntimeChanged?.Invoke(syntaxTree); return; }
        foreach (KeyValuePair<string, string> pair in Config.TypeMap) {
            if (!oldConfig.TypeMap.TryGetValue(pair.Key, out string value))
                { ITSRuntimeChanged?.Invoke(syntaxTree); return; }

            if (pair.Value != value)
                { ITSRuntimeChanged?.Invoke(syntaxTree); return; }
        }
    }

    #endregion


    #region module watcher

    private FileSystemWatcher CreateModuleWatcher(string path) {
        FileSystemWatcher watcher = new(path, "*.d.ts");

        watcher.Changed += OnModuleChanged;
        watcher.Created += OnModuleCreated;
        watcher.Deleted += OnModuleDeleted;
        watcher.Renamed += OnModuleRenamed;

        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        return watcher;
    }

    private void OnModuleChanged(object sender, FileSystemEventArgs e) {
        if (moduleMap.TryGetValue(e.FullPath, out int index)) {
            _ = DoAsync(this, index);

            static async Task DoAsync(TSFileWatcher me, int index) {
                TSModule module = me.syntaxTree.ModuleList[index];
                TSModule localModule = new() {
                    FilePath = module.FilePath,
                    RelativePath = module.RelativePath,
                    ModulePath = module.FilePath,
                    ModuleName = module.ModuleName
                };

                try {
                    await localModule.ParseFunctions();

                    lock (me._lock) {
                        module.FunctionList = localModule.FunctionList;
                        me.ITSRuntimeChanged?.Invoke(me.syntaxTree);
                    }
                }
                catch (IOException) { }
            }
        }
    }

    private void OnModuleCreated(object sender, FileSystemEventArgs e) {
        _ = DoAsync(this, e.FullPath);

        static async Task DoAsync(TSFileWatcher me, string path) {
            try {
                TSModule module = await TSModule.Parse(path, me.Config.DeclarationPath);
            
                lock (me._lock) {
                    me.moduleMap.Add(module.FilePath, me.syntaxTree.ModuleList.Count);
                    me.syntaxTree.ModuleList.Add(module);

                    me.ITSRuntimeChanged?.Invoke(me.syntaxTree);
                }
            }
            catch (IOException) { }
        }
    }

    private void OnModuleDeleted(object sender, FileSystemEventArgs e) {
        if (moduleMap.TryGetValue(e.FullPath, out int index))
            lock (_lock) {
                moduleMap.Remove(e.FullPath);
                syntaxTree.ModuleList.RemoveAt(index);

                ITSRuntimeChanged?.Invoke(syntaxTree);
            }
    }

    private void OnModuleRenamed(object sender, RenamedEventArgs e) {
        if (moduleMap.TryGetValue(e.OldFullPath, out int index)) 
            lock (_lock) {
                moduleMap.Remove(e.OldFullPath);
                moduleMap.Add(e.FullPath, index);
                syntaxTree.ModuleList[index].ParseMetaData(e.FullPath, Config.DeclarationPath);

                ITSRuntimeChanged?.Invoke(syntaxTree);
            }
    }

    #endregion
}
