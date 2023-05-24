using TSRuntime.Core.Configs;

namespace TSRuntime.Core.Parsing;

/// <summary>
/// <para>Represents a parsing result.</para> 
/// <para>The result is a tree like structure, where this is the root and the children are the items in <see cref="ModuleList"/> and <see cref="FunctionList"/>.</para>
/// </summary>
public sealed class TSStructureTree {
    /// <summary>
    /// Contains a list of js-files that are typically imported as modules. 
    /// </summary>
    public List<TSModule> ModuleList { get; set; } = new();
    
    /// <summary>
    /// Contains a list of js-functions that are typically statically included in the html per &lt;script&gt;-tag
    /// </summary>
    public List<TSFunction> FunctionList { get; set; } = new();


    /// <summary>
    /// Traverses recursively the given folder or read the given file and parses every "*.d.ts"-file as <see cref="TSModule"/> and adds it to <see cref="ModuleList"/>.
    /// </summary>
    /// <param name="path">dictionary or file to the source</param>
    public static async Task<TSStructureTree> ParseFiles(string path) {
        TSStructureTree structureTree = new();
        await structureTree.Parse(path);
        return structureTree;
    }

    /// <summary>
    /// Traverses recursively the given folder or read the given file and parses every "*.d.ts"-file as <see cref="TSModule"/> and adds it to <see cref="ModuleList"/>.
    /// </summary>
    /// <param name="path">dictionary or file to the source</param>
    public static async Task<TSStructureTree> ParseFiles(DeclarationPath[] pathList) {
        TSStructureTree structureTree = new();
        await structureTree.Parse(pathList);
        return structureTree;
    }


    /// <summary>
    /// Traverses recursively the given folder or read the given file and parses every "*.d.ts"-file as <see cref="TSModule"/> and adds it to <see cref="ModuleList"/>.
    /// </summary>
    /// <param name="path">dictionary or file to the source</param>
    public Task Parse(string path) => Parse(new DeclarationPath[1] { new(path) });

    /// <summary>
    /// Traverses recursively the given folders or read the given files and parses every "*.d.ts"-file as <see cref="TSModule"/> and adds it to <see cref="ModuleList"/>.
    /// </summary>
    /// <param name="pathList">TODO.</param>
    public async Task Parse(DeclarationPath[] pathList) {
        foreach ((string include, string[] excludes, string? fileModulePath) in pathList) {
            if (File.Exists(include)) {
                string modulePath = fileModulePath switch {
                    string => fileModulePath,
                    _ => include
                };
                if (modulePath.EndsWith(".d.ts"))
                    modulePath = $"{modulePath[..^4]}js";

                ModuleList.Add(await TSModule.ParseWithModulePath(include, modulePath));
            }
            else {
                string[] filePaths = Directory.GetFiles(include, "*.d.ts", SearchOption.AllDirectories)
                    .Select((string filePath) => filePath.Replace('\\', '/'))
                    .Where((string filePath) => DeclarationPath.IsIncluded(filePath, excludes))
                    .ToArray();

                if (ModuleList.Capacity == 0)
                    ModuleList.Capacity = filePaths.Length;

                foreach (string filePath in filePaths)
                    ModuleList.Add(await TSModule.ParseWithRootFolder(filePath, include));
            }
        }
    }
}
