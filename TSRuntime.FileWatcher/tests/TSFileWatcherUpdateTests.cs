using TSRuntime.Core.Configs;
using TSRuntime.FileWatching;
using Xunit;

namespace TSRuntime.FileWatcher.Tests;

public sealed class TSFileWatcherUpdateTests : IAsyncLifetime {
    #region initialization

    private const int FILE_WRITE_DELAY = 1000;

    private const string DECLARATION_FOLDER = ".typescript-declarations";
    private static int testFolderCounter = 0;

    private string rootFolderPath = null!;
    private string declarationPath = null!;
    private TSFileWatcher fileWatcher = null!;


    /**
     * - TempUpdateTestFolder{number}
     *   - .typescript-declarations/   <-- SystemFileWatcher
     **/
    public async Task InitializeAsync() {
        int counter = Interlocked.Increment(ref testFolderCounter);
        
        rootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), $"TempUpdateTestFolder{counter}").Replace('\\', '/');
        Directory.CreateDirectory(rootFolderPath);

        declarationPath = Path.Combine(rootFolderPath, DECLARATION_FOLDER).Replace('\\', '/');
        Directory.CreateDirectory(declarationPath);

        Config config = new() {
            DeclarationPath = [new DeclarationPath(DECLARATION_FOLDER)]
        };

        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
    }

    public Task DisposeAsync() {
        fileWatcher.Dispose();
        Directory.Delete(rootFolderPath, recursive: true);
        return Task.CompletedTask;
    }

    #endregion


    #region helper

    private static async Task<bool> WaitForCondition(Func<bool> condition) {
        await Task.Delay(10);
        
        int retries = 10;
        do {
            if (condition())
                return true;

            retries--;
            await Task.Delay(100);
        }
        while (retries > 0);

        return false;
    }

    #endregion


    #region tsconfig.tsruntime.json

    [Fact]
    public async Task CreateWithMoveConfig_UpdatesStructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CreateWithMoveConfigTemp.json");
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);
        File.Move(configFilePath, Path.Combine(rootFolderPath, TSFileWatcher.JSON_FILE_NAME));

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task CreateConfig_UpdatesStructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string configFilePath = Path.Combine(rootFolderPath, TSFileWatcher.JSON_FILE_NAME);
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task UpdateConfig_UpdatesStructureTree() {
        string configFilePath = Path.Combine(rootFolderPath, TSFileWatcher.JSON_FILE_NAME);
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);
        await Task.Delay(FILE_WRITE_DELAY);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string newConfig = TestFileContent.CONFIG_JSON.Replace(@"""function transform"": ""first upper case""", @"""function transform"": ""first lower case""");
        await File.WriteAllTextAsync(configFilePath, newConfig);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task RemoveConfig_UpdatesStructureTree() {
        string configFilePath = Path.Combine(rootFolderPath, TSFileWatcher.JSON_FILE_NAME);
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);
        await Task.Delay(FILE_WRITE_DELAY);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        File.Delete(configFilePath);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task RenameToConfig_UpdatesStructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string configFilePathWithWrongName = Path.Combine(rootFolderPath, "wrong name");
        await File.WriteAllTextAsync(configFilePathWithWrongName, TestFileContent.CONFIG_JSON);
        await Task.Delay(FILE_WRITE_DELAY);

        string configFilePath = Path.Combine(rootFolderPath, TSFileWatcher.JSON_FILE_NAME);
        File.Move(configFilePathWithWrongName, configFilePath);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task RenameFromConfig_UpdatesStructureTree() {
        string configFilePath = Path.Combine(rootFolderPath, TSFileWatcher.JSON_FILE_NAME);
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);
        await Task.Delay(FILE_WRITE_DELAY);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string configFilePathWithWrongName = Path.Combine(rootFolderPath, "wrong name");
        File.Move(configFilePath, configFilePathWithWrongName);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    #endregion


    #region ts modules

    [Fact]
    public async Task CreateWithMoveFile_UpdatesStructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string sourceFilePath = Path.Combine(rootFolderPath, "createTest.d.ts");
        await File.WriteAllTextAsync(sourceFilePath, TestFileContent.TS_DECLARATION);
        File.Move(sourceFilePath, Path.Combine(declarationPath, "createTest.d.ts"));

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task CreateFile_UpdatesStructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        await File.WriteAllTextAsync(Path.Combine(declarationPath, "updateTest.d.ts"), TestFileContent.TS_DECLARATION);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task UpdateFile_UpdatesStructureTree() {
        string moduleFilePath = Path.Combine(declarationPath, "updateTest.d.ts");
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);
        await Task.Delay(FILE_WRITE_DELAY);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string newContent = TestFileContent.TS_DECLARATION.Replace("export declare function mathJaxRender(): void;", "");
        await File.WriteAllTextAsync(moduleFilePath, newContent);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter >= 1));
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task RemoveFile_UpdatesStructureTree() {
        string moduleFilePath = Path.Combine(declarationPath, "updateTest.d.ts");
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);
        await Task.Delay(FILE_WRITE_DELAY);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        File.Delete(moduleFilePath);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
        Assert.Empty(fileWatcher.StructureTree.ModuleList);
    }

    [Fact]
    public async Task RenameFile_UpdatesStructureTree() {
        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        await Task.Delay(FILE_WRITE_DELAY);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string moduleFilePath2 = Path.Combine(declarationPath, "updateTest2.d.ts");
        File.Move(moduleFilePath1, moduleFilePath2);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    #endregion
}
