namespace TSRuntime.Core.Parsing;

public sealed class SyntaxTree {
    public required List<TSModule> ModuleList { get; set; }
    
    public static SyntaxTree ParseFolder(string folder) {
        string[] filePathes = Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Select((string filePath) => filePath.Replace('\\', '/')).ToArray();
        List<TSModule> moduleList = new(filePathes.Length);

        foreach (string filePath in filePathes) {
            TSModule? tsModule = TSModule.Parse(filePath);
            if (tsModule != null)
                moduleList.Add(tsModule);
        }

        return new SyntaxTree() {
            ModuleList = moduleList
        };
    }
}
