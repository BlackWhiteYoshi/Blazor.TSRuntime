using TSRuntime.Core.Configs;
using TSRuntime.FileWatching;
using Xunit;

namespace TSRuntime.FileWatcher.Tests;

public sealed class TSFileWatcherPathTests : IDisposable {
    #region initialization

    private const int FILE_WRITE_DELAY = 1000;

    private static int testFolderCounter = 0;
    private const string DECLARATION_FOLDER = ".typescript-declarations";
    private const string DECLARATION_FOLDER_NESTED = "nested";
    private const string OTHER_DECLARATION_FOLDER = "other-declarations";

    private readonly string rootFolderPath;
    private readonly string declarationPath;
    private readonly string nestedDeclarationPath;
    private readonly string otherDeclarationPath;
    private TSFileWatcher? fileWatcher;


     /**
     * - TempPathTestFolder{number}
     *   - .typescript-declarations/
     *     - nested/
     *   - other-declarations/
     **/
    public TSFileWatcherPathTests() {
        int counter = Interlocked.Increment(ref testFolderCounter);
        
        rootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), $"TempPathTestFolder{counter}/").Replace('\\', '/');
        Directory.CreateDirectory(rootFolderPath);

        declarationPath = Path.Combine(rootFolderPath, DECLARATION_FOLDER).Replace('\\', '/');
        Directory.CreateDirectory(declarationPath);

        nestedDeclarationPath = Path.Combine(declarationPath, DECLARATION_FOLDER_NESTED).Replace('\\', '/');
        Directory.CreateDirectory(nestedDeclarationPath);

        otherDeclarationPath = Path.Combine(rootFolderPath, OTHER_DECLARATION_FOLDER).Replace('\\', '/');
        Directory.CreateDirectory(otherDeclarationPath);
    }

    public void Dispose() {
        fileWatcher?.Dispose();
        Directory.Delete(rootFolderPath, recursive: true);
    }

    #endregion


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


    #region Include

    [Fact]
    public async Task ModuleFilePath_Works() {
        const string testPath = "something";

        const string moduleName = "updateTest1.d.ts";
        string moduleFilePath = Path.Combine(declarationPath, moduleName);
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);


        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath($"{DECLARATION_FOLDER}/{moduleName}") {
                    FileModulePath = testPath
                }
            }
        };

        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();

        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
        Assert.Equal(testPath, fileWatcher.StructureTree.ModuleList[0].ModuleName);
        Assert.Equal($"/{testPath}.js", fileWatcher.StructureTree.ModuleList[0].ModulePath);
    }

    [Fact]
    public async Task ModuleFilePath_FilePathIsInferred_WhenNull() {
        const string moduleName = "updateTest1.d.ts";
        string moduleFilePath = Path.Combine(declarationPath, moduleName);
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);


        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath($"{DECLARATION_FOLDER}/{moduleName}")
            }
        };

        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();

        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
        Assert.Equal("updateTest1", fileWatcher.StructureTree.ModuleList[0].ModuleName);
        Assert.Equal($"/{DECLARATION_FOLDER}/updateTest1.js", fileWatcher.StructureTree.ModuleList[0].ModulePath);
    }

    [Fact]
    public async Task MultipleIncludes_Works() {
        string moduleFilePath = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);

        string moduleFilePath2 = Path.Combine(otherDeclarationPath, "updateTest2.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);


        Config config = new() {
            DeclarationPath = new DeclarationPath[2] {
                new DeclarationPath(DECLARATION_FOLDER),
                new DeclarationPath(OTHER_DECLARATION_FOLDER)
            }
        };

        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();

        Assert.Equal(2, fileWatcher.StructureTree.ModuleList.Count);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[1].FunctionList.Count);
    }

    [Fact]
    public async Task MultipleMixedIncludes_Works() {
        string moduleFilePath = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);

        const string moduleName = "updateTest2.d.ts";
        string moduleFilePath2 = Path.Combine(otherDeclarationPath, moduleName);
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);


        Config config = new() {
            DeclarationPath = new DeclarationPath[2] {
                new DeclarationPath(DECLARATION_FOLDER),
                new DeclarationPath($"{OTHER_DECLARATION_FOLDER}/{moduleName}") {
                    FileModulePath = string.Empty
                }
            }
        };

        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();

        Assert.Equal(2, fileWatcher.StructureTree.ModuleList.Count);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[1].FunctionList.Count);
    }

    #endregion


    #region Exclude

    [Fact]
    public async Task ExcludeCreateFile_NotUpdateStructureTree() {
        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[1] { $"{DECLARATION_FOLDER}/{DECLARATION_FOLDER_NESTED}" }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;
        Assert.Equal(0, iTsRuntimeChangedCounter);

        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));

        string moduleFilePath2 = Path.Combine(nestedDeclarationPath, "updateTest2.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);
        await Task.Delay(FILE_WRITE_DELAY);

        Assert.Equal(1, iTsRuntimeChangedCounter);
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task ExcludeCreateWithMoveFile_NotUpdateStructureTree() {
        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[1] { $"{DECLARATION_FOLDER}/{DECLARATION_FOLDER_NESTED}" }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;
        Assert.Equal(0, iTsRuntimeChangedCounter);

        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));

        string moduleFilePath2 = Path.Combine(rootFolderPath, "createTest.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);
        File.Move(moduleFilePath2, Path.Combine(nestedDeclarationPath, "createTest.d.ts"));
        await Task.Delay(FILE_WRITE_DELAY);

        Assert.Equal(1, iTsRuntimeChangedCounter);
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task ExcludeUpdateFile_NotUpdateStructureTree() {
        string moduleFilePath2 = Path.Combine(nestedDeclarationPath, "updateTest2.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);

        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[1] { $"{DECLARATION_FOLDER}/{DECLARATION_FOLDER_NESTED}" }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;
        Assert.Equal(0, iTsRuntimeChangedCounter);

        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));

        string newContent = TestFileContent.TS_DECLARATION.Replace("export declare function mathJaxRender(): void;", "");
        await File.WriteAllTextAsync(moduleFilePath2, newContent);
        await Task.Delay(FILE_WRITE_DELAY);

        Assert.Equal(1, iTsRuntimeChangedCounter);
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task ExcludeRemoveFile_NotUpdateStructureTree() {
        string moduleFilePath2 = Path.Combine(nestedDeclarationPath, "updateTest2.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);

        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[1] { $"{DECLARATION_FOLDER}/{DECLARATION_FOLDER_NESTED}" }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;
        Assert.Equal(0, iTsRuntimeChangedCounter);

        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));

        File.Delete(moduleFilePath2);
        await Task.Delay(FILE_WRITE_DELAY);

        Assert.Equal(1, iTsRuntimeChangedCounter);
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task ExcludeRenameFile_NotUpdateStructureTree() {
        string moduleFilePath2 = Path.Combine(nestedDeclarationPath, "updateTest2.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);

        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[1] { $"{DECLARATION_FOLDER}/{DECLARATION_FOLDER_NESTED}" }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;
        Assert.Equal(0, iTsRuntimeChangedCounter);

        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));

        File.Move(moduleFilePath2, moduleFilePath2.Replace('2', '3'));
        await Task.Delay(FILE_WRITE_DELAY);

        Assert.Equal(1, iTsRuntimeChangedCounter);
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task ExcludeRenameToExcludeFile_RemoveModule() {
        string moduleFilePath2 = Path.Combine(declarationPath, "updateTest2.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);

        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[1] { $"{DECLARATION_FOLDER}/{DECLARATION_FOLDER_NESTED}" }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;
        Assert.Equal(0, iTsRuntimeChangedCounter);
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);

        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
        Assert.Equal(2, fileWatcher.StructureTree.ModuleList.Count);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[1].FunctionList.Count);

        File.Move(moduleFilePath2, Path.Combine(nestedDeclarationPath, "updateTest2.d.ts"));
        await Task.Delay(FILE_WRITE_DELAY);

        Assert.Equal(2, iTsRuntimeChangedCounter);
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task ExcludeRenameFromExcludeFile_AddModule() {
        string moduleFilePath2 = Path.Combine(nestedDeclarationPath, "updateTest2.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);

        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[1] { $"{DECLARATION_FOLDER}/{DECLARATION_FOLDER_NESTED}" }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;
        Assert.Equal(0, iTsRuntimeChangedCounter);
        Assert.Empty(fileWatcher.StructureTree.ModuleList);

        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
        
        File.Move(moduleFilePath2, Path.Combine(declarationPath, "updateTest2.d.ts"));
        await Task.Delay(FILE_WRITE_DELAY);

        Assert.Equal(2, iTsRuntimeChangedCounter);
        Assert.Equal(2, fileWatcher.StructureTree.ModuleList.Count);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[1].FunctionList.Count);
    }

    
    [Fact]
    public async Task MultipleExcludes_NotUpdateStructureTree() {
        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[2] {
                        $"{DECLARATION_FOLDER}/{DECLARATION_FOLDER_NESTED}",
                        OTHER_DECLARATION_FOLDER
                    }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;
        Assert.Equal(0, iTsRuntimeChangedCounter);
        Assert.Empty(fileWatcher.StructureTree.ModuleList);

        string moduleFilePath1 = Path.Combine(nestedDeclarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        await Task.Delay(FILE_WRITE_DELAY);
        Assert.Equal(0, iTsRuntimeChangedCounter);
        Assert.Empty(fileWatcher.StructureTree.ModuleList);


        string moduleFilePath2 = Path.Combine(otherDeclarationPath, "updateTest2.d.ts");
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);
        await Task.Delay(FILE_WRITE_DELAY);
        Assert.Equal(0, iTsRuntimeChangedCounter);
        Assert.Empty(fileWatcher.StructureTree.ModuleList);
    }

    [Fact]
    public async Task ExcludeModuleFilePath_NotUpdateStructureTree() {
        const string excludeFileName = "updateTest2.d.ts";
        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(DECLARATION_FOLDER) {
                    Excludes = new string[1] { $"{DECLARATION_FOLDER}/{excludeFileName}" }
                }
            }
        };

        int iTsRuntimeChangedCounter = 0;
        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        fileWatcher.StructureTreeChanged += (_, _) => iTsRuntimeChangedCounter++;

        string moduleFilePath1 = Path.Combine(declarationPath, "updateTest1.d.ts");
        await File.WriteAllTextAsync(moduleFilePath1, TestFileContent.TS_DECLARATION);
        Assert.True(await WaitForCondition(() => iTsRuntimeChangedCounter == 1));
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);

        string moduleFilePath2 = Path.Combine(declarationPath, excludeFileName);
        await File.WriteAllTextAsync(moduleFilePath2, TestFileContent.TS_DECLARATION);
        await Task.Delay(FILE_WRITE_DELAY);

        Assert.Equal(1, iTsRuntimeChangedCounter);
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    [Fact]
    public async Task ExcludeHasNoEffect_WhenModuleFilePath() {
        const string filePath = $"{DECLARATION_FOLDER}/updateTest1.d.ts";

        string moduleFilePath = Path.Combine(rootFolderPath, filePath);
        await File.WriteAllTextAsync(moduleFilePath, TestFileContent.TS_DECLARATION);

        Config config = new() {
            DeclarationPath = new DeclarationPath[1] {
                new DeclarationPath(filePath) {
                    Excludes = new string[2] {
                        DECLARATION_FOLDER,
                        filePath
                    },
                    FileModulePath = string.Empty
                }
            }
        };

        fileWatcher = new TSFileWatcher(config, rootFolderPath);
        await fileWatcher.CreateModuleWatcher();
        Assert.Single(fileWatcher.StructureTree.ModuleList);
        Assert.Equal(5, fileWatcher.StructureTree.ModuleList[0].FunctionList.Count);
    }

    #endregion
}
