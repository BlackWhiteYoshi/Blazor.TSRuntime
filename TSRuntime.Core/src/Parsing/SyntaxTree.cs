namespace TSRuntime.Core.Parsing;

public sealed class SyntaxTree {
    public required List<TSModule> ModuleList { get; set; }
    public required List<TSFunction> FunctionList { get; set; }

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
