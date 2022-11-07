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
    /// Writes <see cref="FilePath"/>, <see cref="RelativePath"/>, <see cref="ModulePath"/> and <see cref="ModuleName"/> by reading meta-data of the given file. 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="rootFolder"></param>
    public void ParseMetaData(string filePath, string rootFolder) {
        FilePath = filePath;
        ReadOnlySpan<char> path = filePath.AsSpan();


        // RelativePath
        if (rootFolder != string.Empty)
            if (rootFolder.EndsWith("/"))
                path = path[rootFolder.Length..];
            else
                path = path[(rootFolder.Length + 1)..];
        RelativePath = path.ToString();


        // ModulePath
        if (path.EndsWith(".d.ts".AsSpan()))
            path = path[..^5]; // skip ".d.ts"

        if (path.StartsWith($"wwwroot/".AsSpan()))
            path = path[8..]; // skip "wwwroot/"

        ModulePath = $"/{path.ToString()}.js";


        // FileName
        int lastSlash = path.LastIndexOf('/');
        ReadOnlySpan<char> rawModuleName = (lastSlash != -1) switch {
            true => path[(lastSlash + 1)..],
            false => path
        };

        if (rawModuleName.EndsWith(".razor".AsSpan()))
            rawModuleName = rawModuleName[..^6]; // skip ".razor"

        if (rawModuleName.Length == 0)
            ModuleName = string.Empty;
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

            ModuleName = saveModuleName.ToString();
        }
    }

    /// <summary>
    /// <para>Parses the file given in <see cref="FilePath"/> and adds the found functions in <see cref="FunctionList"/></para>
    /// <para><see cref="FunctionList"/> is cleared before adding some functions.</para>
    /// </summary>
    public async Task ParseFunctions() {
        FunctionList.Clear();

        using StreamReader streamReader = new(FilePath);
        while (true) {
            string? line = await streamReader.ReadLineAsync();
            if (line == null)
                break;

            TSFunction? tsFunction = TSFunction.Parse(line.AsSpan());
            if (tsFunction != null)
                FunctionList.Add(tsFunction);
        }
    }


    /// <summary>
    /// Creates a <see cref="TSModule"/> with meta-data of the given file and a <see cref="FunctionList">list of js-functions</see> included in the file.
    /// </summary>
    /// <param name="filePath"></param>
    /// /// <param name="rootFolder"></param>
    /// <returns></returns>
    public static async Task<TSModule> Parse(string filePath, string rootFolder) {
        TSModule module = new();

        module.ParseMetaData(filePath, rootFolder);
        await module.ParseFunctions();

        return module;
    }
}
