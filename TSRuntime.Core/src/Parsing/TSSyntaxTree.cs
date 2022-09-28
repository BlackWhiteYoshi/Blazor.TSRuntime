namespace TSRuntime.Core.Parsing;

/// <summary>
/// <para>Represents a parsing result.</para> 
/// <para>It's not a typically syntax-tree, but you can see this as the root and the children are the items in <see cref="ModuleList"/> and <see cref="FunctionList"/>.</para>
/// </summary>
public sealed class TSSyntaxTree {
    /// <summary>
    /// Contains a list of js-files that are typically imported as modules. 
    /// </summary>
    public List<TSModule> ModuleList { get; set; } = new();
    
    /// <summary>
    /// Contains a list of js-functions that are typically statically included in the html per &lt;script&gt;-tag
    /// </summary>
    public List<TSFunction> FunctionList { get; set; } = new();


    /// <summary>
    /// <para>Traverses recursively the given folder and parses every "*.d.ts"-file as <see cref="TSModule"/> and adds it to <see cref="ModuleList"/>.</para>
    /// <para>Before adding items, the <see cref="ModuleList"/> is cleared.</para>
    /// </summary>
    /// <param name="folder">root dictionary where the search begins.</param>
    public void ParseModules(string folder) {
        string[] filePathes = Directory.GetFiles(folder, "*.d.ts", SearchOption.AllDirectories).Select((string filePath) => filePath.Replace('\\', '/')).ToArray();
        
        ModuleList.Clear();
        if (ModuleList.Capacity < filePathes.Length)
            ModuleList.Capacity = filePathes.Length;

        foreach (string filePath in filePathes)
            ModuleList.Add(TSModule.Parse(filePath));
    }

    /// <summary>
    /// <para>Traverses recursively the given folder and searches in every "*.d.ts"-file for js-functions that can be parsed to <see cref="TSFunction"/> and adds these to <see cref="FunctionList"/>.</para>
    /// <para>Before adding items, the <see cref="FunctionList"/> is cleared.</para>
    /// </summary>
    /// <param name="folder"></param>
    public void ParseFunctions(string folder) {
        throw new NotImplementedException("not yet implemented");
    }
}
