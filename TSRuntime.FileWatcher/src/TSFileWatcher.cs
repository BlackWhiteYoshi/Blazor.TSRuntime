using TSRuntime.Core.Configs;
using TSRuntime.Core.Parsing;

namespace TSRuntime.FileWatching;

/// <summary>
/// <para>Watches module folder specified in <see cref="Config.DeclarationPath"/> and the config file tsconfig.tsruntime.json.</para>
/// <para>When a change is detected, <see cref="StructureTree"/> / <see cref="Config"/> is updated accordingly.</para>
/// </summary>
public sealed class TSFileWatcher : IDisposable {
    /// <summary>
    /// The name/path of the json-config file which is a representation of an instance of this class.
    /// </summary>
    public const string JSON_FILE_NAME = "tsconfig.tsruntime.json";


    public TSStructureTree StructureTree { get; } = new();
    private readonly Dictionary<string, int> moduleMap = [];

    public Config Config { get; private set; }
    private readonly string basePath;

    private readonly FileSystemWatcher configWatcher;
    private FileSystemWatcher[]? moduleWatcherList;

    public event Action<Config>? TSRuntimeLocationChanged;
    public event Action<Config>? ITSRuntimeLocationChanged;
    public event Action<Config>? GenerateOnSaveChanged;
    public event Action<TSStructureTree, Config>? StructureTreeChanged;


    /// <summary>
    /// Maps from relative path to absolute path.
    /// </summary>
    /// <param name="declarationPath">content to map</param>
    /// <param name="basePath">absolute path, not empty and without trailing slash.</param>
    /// <returns>A new Array of <see cref="DeclarationPath"/> with absolute paths.</returns>
    public static DeclarationPath[] ConvertToAbsolutePath(DeclarationPath[] declarationPath, string basePath) {
        DeclarationPath[] result = new DeclarationPath[declarationPath.Length];
        for (int i = 0; i < declarationPath.Length; i++) {
            string include = declarationPath[i].Include switch {
                "" => basePath,
                ['/', ..] => $"{basePath}{declarationPath[i].Include}",
                _ => $"{basePath}/{declarationPath[i].Include}"
            };

            string[] excludes = new string[declarationPath[i].Excludes.Length];
            for (int j = 0; j < declarationPath[i].Excludes.Length; j++)
                excludes[j] = declarationPath[i].Excludes[j] switch {
                    "" => basePath,
                    ['/', ..] => $"{basePath}{declarationPath[i].Excludes[j]}",
                    _ => $"{basePath}/{declarationPath[i].Excludes[j]}"
                };

            string? fileModulePath = declarationPath[i].FileModulePath;
            if (fileModulePath == null && File.Exists(include))
                fileModulePath = declarationPath[i].Include;

            result[i] = new DeclarationPath(include, excludes, fileModulePath);
        }

        return result;
    }


    #region Construction

    /// <summary>
    /// <para>Creates an instance of <see cref="TSFileWatcher"/> without initializing the structureTree (inactive).</para>
    /// <para>To activate and initialize the structureTree, call <see cref="CreateModuleWatcher()"/>.</para>
    /// </summary>
    /// <param name="config">config</param>
    /// <param name="basePath">Directory where tsconfig.tsruntime.json file is located.<br />It's also the starting point for relative paths.</param>
    /// <param name="structureTreeChanged">It is set to <see cref="StructureTreeChanged"/> before initialization of the structureTree, so it gets invoked the first time ITSRuntimeContent is build.</param>
    public TSFileWatcher(Config config, string basePath) {
        Config = config;
        this.basePath = basePath;
        configWatcher = CreateConfigWatcher(basePath);
    }

    public void Dispose() {
        configWatcher.Dispose();
        DisposeModuleWatcher();
    }

    #endregion


    #region StructureTree

    private async Task CreateStructureTree(DeclarationPath[] declarationPathList) {
        TSStructureTree localStructureTree = await TSStructureTree.ParseFiles(declarationPathList);

        lock (StructureTree) {
            StructureTree.ModuleList.Clear();
            StructureTree.ModuleList.AddRange(localStructureTree.ModuleList);
            moduleMap.Clear();
            for (int i = 0; i < StructureTree.ModuleList.Count; i++)
                moduleMap.Add(localStructureTree.ModuleList[i].FilePath, i);

            StructureTreeChanged?.Invoke(StructureTree, Config);
        }
    }

    private void AddModule(TSModule module) {
        lock (StructureTree) {
            moduleMap.Add(module.FilePath, StructureTree.ModuleList.Count);
            StructureTree.ModuleList.Add(module);

            StructureTreeChanged?.Invoke(StructureTree, Config);
        }
    }

    private void RemoveModule(string filePath) {
        if (moduleMap.TryGetValue(filePath, out int index)) {
            lock (StructureTree) {
                StructureTree.ModuleList.RemoveAt(index);
                moduleMap.Remove(filePath);

                StructureTreeChanged?.Invoke(StructureTree, Config);
            }
        }
    }

    private void UpdateModule(TSModule module) {
        if (moduleMap.TryGetValue(module.FilePath, out int index)) {
            TSModule dirtyModule = StructureTree.ModuleList[index];
            lock (StructureTree) {
                dirtyModule.FunctionList = module.FunctionList;

                StructureTreeChanged?.Invoke(StructureTree, Config);
            }
        }
        else
            AddModule(module);
    }

    #endregion


    #region config watcher

    private FileSystemWatcher CreateConfigWatcher(string path) {
        FileSystemWatcher watcher = new(path, JSON_FILE_NAME);

        watcher.Changed += OnConfigChanged;
        watcher.Created += OnConfigCreated;
        watcher.Deleted += OnConfigDeleted;
        watcher.Renamed += OnConfigRenamed;

        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

        watcher.IncludeSubdirectories = false;
        watcher.EnableRaisingEvents = true;

        return watcher;


        void OnConfigChanged(object sender, FileSystemEventArgs e) => UpdateConfig(e.FullPath.Replace('\\', '/'));

        void OnConfigCreated(object sender, FileSystemEventArgs e) => UpdateConfig(e.FullPath.Replace('\\', '/'));

        void OnConfigDeleted(object sender, FileSystemEventArgs e) => _ = UpdateConfig(new Config());

        void OnConfigRenamed(object sender, RenamedEventArgs e) {
            if (e.Name == JSON_FILE_NAME)
                UpdateConfig(e.FullPath.Replace('\\', '/'));
            else
                _ = UpdateConfig(new Config());
        }
    }


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

            await me.UpdateConfig(new Config(json));
        }
    }

    private Task UpdateConfig(Config config) {
        Config oldConfig = Config;
        Config = config;

        if (Config.FileOutputClass != oldConfig.FileOutputClass)
            TSRuntimeLocationChanged?.Invoke(Config);

        if (Config.FileOutputinterface != oldConfig.FileOutputinterface)
            ITSRuntimeLocationChanged?.Invoke(Config);

        if (Config.GenerateOnSave != oldConfig.GenerateOnSave)
            GenerateOnSaveChanged?.Invoke(Config);
        
        if (moduleWatcherList == null)
            return Task.CompletedTask;


        if (Config.DeclarationPath != oldConfig.DeclarationPath) {
            DisposeModuleWatcher();
            return CreateModuleWatcher();
        }

        if (!Config.StructureTreeEquals(oldConfig))
            StructureTreeChanged?.Invoke(StructureTree, Config);

        return Task.CompletedTask;
    }

    #endregion


    #region module watcher

    /// <summary>
    /// Activates the fileWatcher: Creates a <see cref="FileSystemWatcher"/> for every declaraionPath and initializes the structureTree.
    /// </summary>
    /// <returns></returns>
    public Task CreateModuleWatcher() {
        if (moduleWatcherList != null)
            return Task.CompletedTask;

        DeclarationPath[] declarationPathList = ConvertToAbsolutePath(Config.DeclarationPath, basePath);

        moduleWatcherList = new FileSystemWatcher[Config.DeclarationPath.Length];
        for (int i = 0; i < Config.DeclarationPath.Length; i++)
            moduleWatcherList[i] = CreateModuleWatcher(declarationPathList[i]);

        return CreateStructureTree(declarationPathList);
    }

    /// <summary>
    /// Deactivates the fileWatcher: Disposes all declarationPath <see cref="FileSystemWatcher"/>.
    /// </summary>
    public void DisposeModuleWatcher() {
        if (moduleWatcherList == null)
            return;

        foreach (FileSystemWatcher moduleWatcher in moduleWatcherList)
            moduleWatcher.Dispose();
        moduleWatcherList = null;
    }

    private FileSystemWatcher CreateModuleWatcher(DeclarationPath path) {
        FileSystemWatcher watcher = path.FileModulePath switch {
            // watching folder
            null => new FileSystemWatcher(path.Include, "*.d.ts"),
            // watching file
            _ => new FileSystemWatcher(Path.GetDirectoryName(path.Include), Path.GetFileName(path.Include))
        };

        watcher.Changed += OnModuleChanged;
        watcher.Created += OnModuleCreated;
        watcher.Deleted += OnModuleDeleted;
        watcher.Renamed += OnModuleRenamed;

        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        return watcher;


        void OnModuleChanged(object sender, FileSystemEventArgs e) {
            string filePath = e.FullPath.Replace('\\', '/');
            if (!DeclarationPath.IsIncluded(filePath, path.Excludes))
                return;

            if (moduleMap.TryGetValue(filePath, out int index))
                AddToDirtyList(StructureTree.ModuleList[index]);
        }

        void OnModuleCreated(object sender, FileSystemEventArgs e) {
            string filePath = e.FullPath.Replace('\\', '/');
            if (!DeclarationPath.IsIncluded(filePath, path.Excludes))
                return;

            TSModule module = new();
            if (path.FileModulePath == null)
                module.ParseMetaDataRootFolder(filePath, path.Include);
            else
                module.ParseMetaDataModulePath(filePath, path.FileModulePath);

            AddToDirtyList(module);
        }

        void OnModuleDeleted(object sender, FileSystemEventArgs e) {
            string filePath = e.FullPath.Replace('\\', '/');
            if (!DeclarationPath.IsIncluded(filePath, path.Excludes))
                return;

            RemoveModule(filePath);
        }

        void OnModuleRenamed(object sender, RenamedEventArgs e) {
            string oldPath = e.OldFullPath.Replace('\\', '/');
            string newPath = e.FullPath.Replace('\\', '/');

            bool isIncludedOld = DeclarationPath.IsIncluded(oldPath, path.Excludes);
            bool isIncludedNew = DeclarationPath.IsIncluded(newPath, path.Excludes);
            switch ((isIncludedOld, isIncludedNew)) {
                case (false, false):
                    return;

                case (true, false):
                    RemoveModule(oldPath);
                    break;

                case (false, true):
                    TSModule module = new();
                    if (path.FileModulePath == null)
                        module.ParseMetaDataRootFolder(newPath, path.Include);
                    else
                        module.ParseMetaDataModulePath(newPath, path.FileModulePath);

                    AddToDirtyList(module);
                    break;

                case (true, true):
                    int index = moduleMap[oldPath];
                    lock (StructureTree) {
                        moduleMap.Remove(oldPath);
                        moduleMap.Add(newPath, index);
                        if (path.FileModulePath == null)
                            StructureTree.ModuleList[index].ParseMetaDataRootFolder(newPath, path.Include);
                        else
                            StructureTree.ModuleList[index].ParseMetaDataModulePath(newPath, path.FileModulePath);
                    }

                    break;
            }
            
            StructureTreeChanged?.Invoke(StructureTree, Config);
        }
    }


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
    private readonly HashSet<TSModule> dirtyModules = [];
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

                UpdateModule(module);
            }

            await Task.Delay(1000);
        }
    }

    #endregion
}
