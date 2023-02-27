using TSRuntime.Core.Configs;
using TSRuntime.Core.Parsing;
using TSRuntime.FileWatching;
using Xunit;

namespace TSRuntime.FileWatcher.Tests;

public sealed class TSFileWatcherTests : IAsyncLifetime {
    #region initialization

    private const string DECLARATION_PATH = ".typescript-declarations/";
    private static int testFolderCounter = 0;

    private string folderPath = null!;
    private string declarationPath = null!;
    private TSFileWatcher fileWatcher = null!;


    public Task InitializeAsync() {
        int counter = Interlocked.Increment(ref testFolderCounter);
        
        folderPath = Path.Combine(Directory.GetCurrentDirectory(), $"TempTestFolder{counter}/").Replace('\\', '/');
        Directory.CreateDirectory(folderPath);

        declarationPath = Path.Combine(folderPath, DECLARATION_PATH).Replace('\\', '/');
        Directory.CreateDirectory(declarationPath);

        Config config = new() {
            DeclarationPath = DECLARATION_PATH
        };

        fileWatcher = new TSFileWatcher(config, folderPath);
        return fileWatcher.CreateStructureTree();
    }

    public Task DisposeAsync() {
        fileWatcher.Dispose();
        Directory.Delete(folderPath, recursive: true);
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
    public async Task CreateWithMoveConfig_Updates_StructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += _ => iTsRuntimeChangedCounter++;

        string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CreateWithMoveConfigTemp.json");
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);
        File.Move(configFilePath, Path.Combine(folderPath, Config.JSON_FILE_NAME));

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task CreateConfig_Updates_StructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += _ => iTsRuntimeChangedCounter++;

        string configFilePath = Path.Combine(folderPath, Config.JSON_FILE_NAME);
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task UpdateConfig_Updates_StructureTree() {
        string configFilePath = Path.Combine(folderPath, Config.JSON_FILE_NAME);
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);
        await Task.Delay(2000);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += _ => iTsRuntimeChangedCounter++;

        string newConfig = TestFileContent.CONFIG_JSON.Replace(@"""function transform"": ""first upper case""", @"""function transform"": ""first lower case""");
        await File.WriteAllTextAsync(configFilePath, newConfig);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task RemoveConfig_Updates_StructureTree() {
        string configFilePath = Path.Combine(folderPath, Config.JSON_FILE_NAME);
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);
        await Task.Delay(2000);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += _ => iTsRuntimeChangedCounter++;

        File.Delete(configFilePath);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task RenameToConfig_Updates_StructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += _ => iTsRuntimeChangedCounter++;

        string configFilePathWithWrongName = Path.Combine(folderPath, "wrong name");
        await File.WriteAllTextAsync(configFilePathWithWrongName, TestFileContent.CONFIG_JSON);
        await Task.Delay(2000);

        string configFilePath = Path.Combine(folderPath, Config.JSON_FILE_NAME);
        File.Move(configFilePathWithWrongName, configFilePath);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task RenameFromConfig_Updates_StructureTree() {
        string configFilePath = Path.Combine(folderPath, Config.JSON_FILE_NAME);
        await File.WriteAllTextAsync(configFilePath, TestFileContent.CONFIG_JSON);
        await Task.Delay(2000);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += _ => iTsRuntimeChangedCounter++;

        string configFilePathWithWrongName = Path.Combine(folderPath, "wrong name");
        File.Move(configFilePath, configFilePathWithWrongName);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    #endregion


    #region ts modules

    [Fact]
    public async Task CreateWithMoveFile_Updates_StructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += (TSStructureTree structureTree) => {
            Assert.Single(structureTree.ModuleList);
            Assert.Equal(6, structureTree.ModuleList[0].FunctionList.Count);

            iTsRuntimeChangedCounter++;
        };

        string sourceFilePath = Path.Combine(folderPath, "createTest.d.ts");
        await File.WriteAllTextAsync(sourceFilePath, TestFileContent.TS_DECLARATION);
        File.Move(sourceFilePath, Path.Combine(declarationPath, "createTest.d.ts"));

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));

    }

    [Fact]
    public async Task CreateFile_Updates_StructureTree() {
        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += (TSStructureTree structureTree) => {
            Assert.Single(structureTree.ModuleList);
            Assert.Equal(6, structureTree.ModuleList[0].FunctionList.Count);

            iTsRuntimeChangedCounter++;
        };

        await File.WriteAllTextAsync(Path.Combine(declarationPath, "updateTest.d.ts"), TestFileContent.TS_DECLARATION);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task UpdateFile_Updates_StructureTree() {
        string moduleFilePath = Path.Combine(declarationPath, "updateTest.d.ts");
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);
        await Task.Delay(2000);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += (TSStructureTree structureTree) => {
            Assert.Single(structureTree.ModuleList);
            Assert.Equal(5, structureTree.ModuleList[0].FunctionList.Count);

            iTsRuntimeChangedCounter++;
        };

        string newModule = TestFileContent.TS_DECLARATION.Replace("export declare function mathJaxRender(): void;", "");
        await File.WriteAllTextAsync(moduleFilePath, newModule);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter >= 1));
    }

    [Fact]
    public async Task RemoveFile_Updates_StructureTree() {
        string moduleFilePath = Path.Combine(declarationPath, "updateTest.d.ts");
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);
        await Task.Delay(2000);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += (TSStructureTree structureTree) => {
            Assert.Empty(structureTree.ModuleList);

            iTsRuntimeChangedCounter++;
        };

        File.Delete(moduleFilePath);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    [Fact]
    public async Task RenameFile_Updates_StructureTree() {
        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        await Task.Delay(2000);

        int iTsRuntimeChangedCounter = 0;
        fileWatcher.ITSRuntimeChanged += (TSStructureTree structureTree) => {
            Assert.Single(structureTree.ModuleList);
            Assert.Equal(6, structureTree.ModuleList[0].FunctionList.Count);

            iTsRuntimeChangedCounter++;
        };

        string moduleFilePath2 = Path.Combine(declarationPath, "updateTest2.d.ts");
        File.Move(moduleFilePath1, moduleFilePath2);

        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
    }

    #endregion
}
