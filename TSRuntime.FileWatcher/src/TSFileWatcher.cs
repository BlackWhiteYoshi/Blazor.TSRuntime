using TSRuntime.Core.Configs;
using TSRuntime.Core.Parsing;

namespace TSRuntime.FileWatching;

/// <summary>
/// <para>Watches module folder specified in <see cref="Config.DeclarationPath"/> and the config file tsconfig.tsruntime.json.</para>
/// <para>When a change is detected, <see cref="structureTree"/> / <see cref="Config"/> is updated accordingly.</para>
/// </summary>
public sealed class TSFileWatcher : IDisposable {
    private readonly FileSystemWatcher configWatcher;
    private readonly FileSystemWatcher moduleWatcher;

    private readonly TSStructureTree structureTree = new();
    private readonly Dictionary<string, int> moduleMap = new();


    private readonly string basePath;
    private string declarationPath;
    public Config Config { get; private set; }

    public event Action? TSRuntimeLocationChanged;
    public event Action? ITSRuntimeLocationChanged;
    public event Action<TSStructureTree>? ITSRuntimeChanged;


    /// <summary>
    /// <para>Creates an instance of <see cref="TSFileWatcher"/> without proactive initializing the structureTree.</para>
    /// <para>To initialize the structureTree, call <see cref="CreateStructureTree"/>.</para>
    /// </summary>
    /// <param name="config">If null, default config is supplied.</param>
    /// <param name="basePath">Directory where tsconfig.tsruntime.json file is located.<br />It's also the starting point for relative pathes.</param>
    public TSFileWatcher(Config? config, string basePath) {
        this.basePath = basePath;
        if (config != null)
            Config = config;
        else
            Config = new();

        declarationPath = Path.Combine(basePath, Config.DeclarationPath);
        configWatcher = CreateConfigWatcher(basePath);
        moduleWatcher = CreateModuleWatcher(declarationPath);
    }

    public void Dispose() {
        configWatcher.Dispose();
        moduleWatcher.Dispose();
    }


    /// <summary>
    /// Parses all files and recreate the structureTree.
    /// </summary>
    /// <returns></returns>
    public async Task CreateStructureTree() {
        TSStructureTree localStructureTree = new();
        await localStructureTree.ParseModules(declarationPath);

        lock (structureTree) {
            structureTree.ModuleList.Clear();
            moduleMap.Clear();
            structureTree.ModuleList.AddRange(localStructureTree.ModuleList);
            for (int i = 0; i < structureTree.ModuleList.Count; i++)
                moduleMap.Add(localStructureTree.ModuleList[i].FilePath, i);

            ITSRuntimeChanged?.Invoke(structureTree);
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

        watcher.IncludeSubdirectories = false;
        watcher.EnableRaisingEvents = true;

        return watcher;
    }


    private void OnConfigChanged(object sender, FileSystemEventArgs e) => UpdateConfig(e.FullPath.Replace('\\', '/'));

    private void OnConfigCreated(object sender, FileSystemEventArgs e) => UpdateConfig(e.FullPath.Replace('\\', '/'));

    private void OnConfigDeleted(object sender, FileSystemEventArgs e) => _ = UpdateConfig(new Config());

    private void OnConfigRenamed(object sender, RenamedEventArgs e) {
        if (e.Name == Config.JSON_FILE_NAME)
            UpdateConfig(e.FullPath.Replace('\\', '/'));
        else
            UpdateConfig(new Config());
    }


    #region UpdateConfig

    private Task configUpdater = Task.CompletedTask;

    private void UpdateConfig(string jsonPath) {
        if (configUpdater.IsCompleted)
            configUpdater = UpdateConfig(this, jsonPath);
        else if (configUpdater.IsFaulted)
            throw configUpdater.Exception;

        static async Task UpdateConfig(TSFileWatcher me, string jsonPath) {
            await Task.Delay(500);

            string json;
            while (true) {
                try {
                    using StreamReader streamReader = new(jsonPath);
                    json = await streamReader.ReadToEndAsync();
                    break;
                }
                catch (IOException) {
                    await Task.Delay(1000);
                }
            }

            await me.UpdateConfig(Config.FromJson(json));
        }
    }
    
    private Task UpdateConfig(Config config) {
        Config oldConfig = Config;
        Config = config;


        if (Config.FileOutputClass != oldConfig.FileOutputClass)
            TSRuntimeLocationChanged?.Invoke();

        if (Config.FileOutputinterface != oldConfig.FileOutputinterface) 
            ITSRuntimeLocationChanged?.Invoke();

        if (Config.DeclarationPath != oldConfig.DeclarationPath) {
            declarationPath = Path.Combine(basePath, Config.DeclarationPath);
            return CreateStructureTree();
        }
        
        if (Config.ModuleInvokeEnabled != oldConfig.ModuleInvokeEnabled)
            return CreateStructureTree();

        if (Config.ModuleTrySyncEnabled != oldConfig.ModuleTrySyncEnabled)
            return CreateStructureTree();

        if (Config.ModuleAsyncEnabled != oldConfig.ModuleAsyncEnabled)
            return CreateStructureTree();

        if (Config.JSRuntimeInvokeEnabled != oldConfig.JSRuntimeInvokeEnabled)
            return CreateStructureTree();

        if (Config.JSRuntimeTrySyncEnabled != oldConfig.JSRuntimeTrySyncEnabled)
            return CreateStructureTree();

        if (Config.JSRuntimeAsyncEnabled != oldConfig.JSRuntimeAsyncEnabled)
            return CreateStructureTree();

        if (Config.FunctionNamePattern != oldConfig.FunctionNamePattern)
            return CreateStructureTree();

        if (Config.UsingStatements.Length != oldConfig.UsingStatements.Length)
            return CreateStructureTree();
        for (int i = 0; i < Config.UsingStatements.Length; i++)
            if (Config.UsingStatements[i] != oldConfig.UsingStatements[i])
                return CreateStructureTree();

        if (Config.TypeMap.Count != oldConfig.TypeMap.Count)
            return CreateStructureTree();
        foreach (KeyValuePair<string, string> pair in Config.TypeMap) {
            if (!oldConfig.TypeMap.TryGetValue(pair.Key, out string value))
                return CreateStructureTree();

            if (pair.Value != value)
                return CreateStructureTree();
        }

        return Task.CompletedTask;
    }

    #endregion

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
        if (moduleMap.TryGetValue(e.FullPath.Replace('\\', '/'), out int index)) {
            TSModule module = structureTree.ModuleList[index];
            TSModule localModule = new() {
                FilePath = module.FilePath,
                RelativePath = module.RelativePath,
                ModulePath = module.FilePath,
                ModuleName = module.ModuleName
            };

            AddToDirtyList(localModule);
        }
    }

    private void OnModuleCreated(object sender, FileSystemEventArgs e) {
        TSModule module = new();
        module.ParseMetaData(e.FullPath.Replace('\\', '/'), declarationPath);
        
        AddToDirtyList(module);
    }

    private void OnModuleDeleted(object sender, FileSystemEventArgs e) {
        string path = e.FullPath.Replace('\\', '/');
        if (moduleMap.TryGetValue(path, out int index)) {
            lock (structureTree) {
                moduleMap.Remove(path);
                structureTree.ModuleList.RemoveAt(index);
            }

            _ = CreateStructureTree();
        }
    }

    private void OnModuleRenamed(object sender, RenamedEventArgs e) {
        string oldPath = e.OldFullPath.Replace('\\', '/');
        string newPath = e.FullPath.Replace('\\', '/');
        if (moduleMap.TryGetValue(oldPath, out int index)) {
            lock (structureTree) {
                moduleMap.Remove(oldPath);
                moduleMap.Add(newPath, index);
                structureTree.ModuleList[index].ParseMetaData(newPath, declarationPath);
            }

            _ = CreateStructureTree();
        }
    }


    #region Module Updater

    private void AddToDirtyList(TSModule module) {
        lock (dirtyModules) {
            if (dirtyModules.Add(module))
                if (moduleUpdater.IsCompleted)
                    moduleUpdater = UpdateModules();
                else if (moduleUpdater.IsFaulted)
                    throw moduleUpdater.Exception;
        }
    }

    /// <summary>
    /// contains new and local modules, so only modules that are not in structureTree
    /// </summary>
    private readonly HashSet<TSModule> dirtyModules = new();
    private Task moduleUpdater = Task.CompletedTask;
    
    private async Task UpdateModules() {
        await Task.Delay(500);

        while (dirtyModules.Count > 0) {
            foreach (TSModule module in dirtyModules) {
                try {
                    await module.ParseFunctions();
                    lock (dirtyModules)
                        dirtyModules.Remove(module);
                }
                catch (IOException) {
                    continue;
                }

                if (moduleMap.TryGetValue(module.FilePath, out int index)) {
                    // changed module, update the corresponding one in structureTree
                    TSModule dirtyModule = structureTree.ModuleList[index];
                    lock (structureTree) {
                        dirtyModule.FunctionList = module.FunctionList;
                    }
                    await CreateStructureTree();
                }
                else {
                    // new module, add to structureTree
                    lock (structureTree) {
                        moduleMap.Add(module.FilePath, structureTree.ModuleList.Count);
                        structureTree.ModuleList.Add(module);
                    }
                    await CreateStructureTree();
                }
            }

            await Task.Delay(1000);
        }
    }

    #endregion

    #endregion
}
