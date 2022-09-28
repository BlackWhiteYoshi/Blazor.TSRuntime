using TSRuntime.Core.Configs;

namespace TSRuntime.Core.Parsing;

/// <summary>
/// Represents a js-module (a js-file loaded as module).
/// </summary>
public sealed class TSModule {
    /// <summary>
    /// The raw given filePath to the module.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// The <see cref="FilePath"/> without starting <see cref="Config.DECLARATION_PATH"/>.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// The <see cref="RelativePath"/> but starts with "/" and ends with ".js", also ignoring starting "/wwwroot".
    /// </summary>
    public string ModulePath { get; set; } = string.Empty;

    /// <summary>
    /// fileName without ending ".d.ts" or ".razor" and not allowed variable-characters are replaced with '_'.
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// List of js-functions of a module (js-file).
    /// </summary>
    public List<TSFunction> FunctionList { get; set; } = new();


    /// <summary>
    /// Creates a <see cref="TSModule"/> with meta-data of the given file and a <see cref="FunctionList">list of js-functions</see> included in the file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
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
