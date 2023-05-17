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
    /// <para>Traverses recursively the given folder or read the given file and parses every "*.d.ts"-file as <see cref="TSModule"/> and adds it to <see cref="ModuleList"/>.</para>
    /// <para>Before adding items, the <see cref="ModuleList"/> is cleared.</para>
    /// </summary>
    /// <param name="folder">root dictionary where the search begins.</param>
    public Task ParseModules(string path) => ParseModules(new DeclarationPath[1] { new(path) });

    /// <summary>
    /// <para>Traverses recursively the given folders or read the given files and parses every "*.d.ts"-file as <see cref="TSModule"/> and adds it to <see cref="ModuleList"/>.</para>
    /// <para>Before adding items, the <see cref="ModuleList"/> is cleared.</para>
    /// </summary>
    /// <param name="folder">root dictionary where the search begins.</param>
    public async Task ParseModules(DeclarationPath[] pathList) {
        ModuleList.Clear();

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


    /// <summary>
    /// <para>Traverses recursively the given folder or read the given file and searches in every "*.d.ts"-file for js-functions that can be parsed to <see cref="TSFunction"/> and adds these to <see cref="FunctionList"/>.</para>
    /// <para>Before adding items, the <see cref="FunctionList"/> is cleared.</para>
    /// </summary>
    /// <param name="path"></param>
    public void ParseFunctions(string path) => ParseFunctions(new DeclarationPath[1] { new(path) });

    /// <summary>
    /// <para>Traverses recursively the given folders or read the given files and searches in every "*.d.ts"-file for js-functions that can be parsed to <see cref="TSFunction"/> and adds these to <see cref="FunctionList"/>.</para>
    /// <para>Before adding items, the <see cref="FunctionList"/> is cleared.</para>
    /// </summary>
    /// <param name="path"></param>
    public void ParseFunctions(DeclarationPath[] pathList) {
        throw new NotImplementedException("not yet implemented");
    }
}
