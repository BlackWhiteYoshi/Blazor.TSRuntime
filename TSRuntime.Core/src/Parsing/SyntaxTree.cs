namespace TSRuntime.Core.Parsing;

public sealed class SyntaxTree {
    public List<TSModule> ModuleList { get; set; } = new();
    public List<TSFunction> FunctionList { get; set; } = new();

    public void ParseModules(string folder) {
        string[] filePathes = Directory.GetFiles(folder, "*.d.ts", SearchOption.AllDirectories).Select((string filePath) => filePath.Replace('\\', '/')).ToArray();
        
        ModuleList.Clear();
        ModuleList.EnsureCapacity(filePathes.Length);

        foreach (string filePath in filePathes)
            ModuleList.Add(TSModule.Parse(filePath));
    }

    public void ParseFunctions(string folder) {
        throw new NotImplementedException("not yet implemented");
    }
}
