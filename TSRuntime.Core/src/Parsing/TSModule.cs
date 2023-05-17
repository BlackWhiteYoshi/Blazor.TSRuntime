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


    #region ParseMetaData

    /// <summary>
    /// Writes <see cref="FilePath"/>, <see cref="ModulePath"/> and <see cref="ModuleName"/> by reading meta-data of the given file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="rootFolder"></param>
    public void ParseMetaDataRootFolder(string filePath, string rootFolder) {
        FilePath = filePath;
        ReadOnlySpan<char> path = filePath.AsSpan();

        // relative path
        if (rootFolder != string.Empty)
            if (rootFolder is [.., '/'])
                path = path[rootFolder.Length..];
            else
                path = path[(rootFolder.Length + 1)..];

        // ModulePath
        if (path.EndsWith(".d.ts".AsSpan()))
            path = path[..^5]; // skip ".d.ts"

        if (path.StartsWith($"wwwroot/".AsSpan()))
            path = path[8..]; // skip "wwwroot/"

        ModulePath = $"/{path.ToString()}.js";

        // ModuleName
        ParseModuleName(path);
    }

    /// <summary>
    /// Writes <see cref="FilePath"/>, <see cref="ModulePath"/> and <see cref="ModuleName"/> by reading meta-data of the given file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="modulePath"></param>
    public void ParseMetaDataModulePath(string filePath, string modulePath) {
        FilePath = filePath;

        // ModulePath
        ReadOnlySpan<char> path;
        switch (modulePath.StartsWith("/"), modulePath.EndsWith(".js")) {
            case (true, true):
                ModulePath = modulePath;
                path = modulePath.AsSpan(1, modulePath.Length - 4);
                break;
            case (false, true):
                ModulePath = $"/{modulePath}";
                path = modulePath.AsSpan(0, modulePath.Length - 3);
                break;
            case (true, false):
                ModulePath = $"{modulePath}.js";
                path = modulePath.AsSpan(1, modulePath.Length - 1);
                break;
            case (false, false):
                ModulePath = $"/{modulePath}.js";
                path = modulePath.AsSpan();
                break;
        };

        // ModuleName
        ParseModuleName(path);
    }

    /// <summary>
    /// Retrieves the file name of the path (without ".razor") and replaces unsafe characters with "_".
    /// </summary>
    /// <param name="path">relative path of the module without starting "/" and ending ".js"</param>
    private void ParseModuleName(ReadOnlySpan<char> path) {
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

    #endregion


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
    /// <param name="rootFolder"></param>
    /// <returns></returns>
    public static async Task<TSModule> ParseWithRootFolder(string filePath, string rootFolder) {
        TSModule module = new();

        module.ParseMetaDataRootFolder(filePath, rootFolder);
        await module.ParseFunctions();

        return module;
    }

    /// <summary>
    /// Creates a <see cref="TSModule"/> with meta-data of the given file and a <see cref="FunctionList">list of js-functions</see> included in the file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="modulePath"></param>
    /// <returns></returns>
    public static async Task<TSModule> ParseWithModulePath(string filePath, string modulePath) {
        TSModule module = new();

        module.ParseMetaDataModulePath(filePath, modulePath);
        await module.ParseFunctions();

        return module;
    }
}
