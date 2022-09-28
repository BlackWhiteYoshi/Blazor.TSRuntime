using TSRuntime.Core.Configs;

namespace TSRuntime.Core.Parsing;

public sealed class TSModule {
    public string FilePath { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ModulePath { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public List<TSFunction> FunctionList { get; set; } = new();


    public static TSModule Parse(string filePath) {
        TSModule module = new() {
            FilePath = filePath
        };

        ReadOnlySpan<char> path = filePath.AsSpan();


        // RelativePath
        path = path[Config.DECLARATION_PATH.Length..];
        module.RelativePath = path.ToString();


        // ModulePath
        path = path[..^5]; // skip ".d.ts"

        if (path.StartsWith($"wwwroot/".AsSpan()))
            path = path[8..];

        module.ModulePath = $"/{path.ToString()}.js";


        // FileName
        int lastSlash = path.LastIndexOf('/');
        ReadOnlySpan<char> rawModuleName = (lastSlash != -1) switch {
            true => path[(lastSlash + 1)..],
            false => path
        };

        if (rawModuleName.EndsWith(".razor".AsSpan()))
            rawModuleName = rawModuleName[..^6]; // skip ".razor"

        if (rawModuleName.Length == 0)
            module.ModuleName = string.Empty;
        else {
            Span<char> saveModuleName = stackalloc char[rawModuleName.Length + 1];
            int startIndex;
            if (char.IsDigit(rawModuleName[0])) {
                saveModuleName[0] = '_';
                startIndex = 1;
            }
            else {
                saveModuleName = saveModuleName[1..];
                startIndex = 0;
            }
            for (int i = startIndex; i < saveModuleName.Length; i++)
                saveModuleName[i] = char.IsLetterOrDigit(rawModuleName[i]) switch {
                    true => rawModuleName[i],
                    false => '_'
                };

            module.ModuleName = saveModuleName.ToString();
        }


        // FunctionList
        foreach (string line in File.ReadLines(filePath)) {
            TSFunction? tsFunction = TSFunction.Parse(line.AsSpan());
            if (tsFunction != null)
                module.FunctionList.Add(tsFunction);
        }


        return module;
    }
}
