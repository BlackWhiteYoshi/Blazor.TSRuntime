namespace TSRuntime.Core.Parsing;

public sealed class SyntaxTree {
    public required List<TSModule> ModuleList { get; set; }
    
    public static SyntaxTree ParseFolder(string folder) {
        string[] filePathes = Directory.GetFiles(folder, "*.d.ts", SearchOption.AllDirectories).Select((string filePath) => filePath.Replace('\\', '/')).ToArray();
        List<TSModule> moduleList = new(filePathes.Length);

        foreach (string filePath in filePathes)
            moduleList.Add(TSModule.Parse(filePath));

        return new SyntaxTree() {
            ModuleList = moduleList
        };
    }
}
