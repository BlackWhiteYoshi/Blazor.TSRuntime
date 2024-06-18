namespace TSRuntime.Parsing;

/// <summary>
/// Represents a js/ts/d.ts-file.
/// </summary>
public abstract class TSFile : IEquatable<TSFile> {
    /// <summary>
    /// The raw given filePath to the module.
    /// </summary>
    public string FilePath { get; protected init; } = string.Empty;

    /// <summary>
    /// The <see cref="FilePath"/> but it is relative, starts with "/" and ends with ".js", also ignoring starting "/wwwroot".
    /// </summary>
    public string URLPath { get; protected init; } = string.Empty;

    /// <summary>
    /// fileName without ending ".d.ts/.ts/.js" or ".razor" and not allowed variable-characters are replaced with '_'.
    /// </summary>
    public string Name { get; protected init; } = string.Empty;

    /// <summary>
    /// List of js-functions of the module/script.
    /// </summary>
    public IReadOnlyList<TSFunction> FunctionList { get; protected init; } = [];


    /// <summary>
    /// removes extension ".js"/".ts"/".d.ts", skips leading "wwwroot" and makes sure it starts with '/'.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected string CreateURLPath(ref ReadOnlySpan<char> path) {
        path = path switch {
            [.., '.', 'd', '.', 't', 's'] => path[..^5], // skip ".d.ts"
            [.., '.', 'j', 's'] or [.., '.', 't', 's'] => path[..^3], // skip ".js"/".ts"
            _ => throw new Exception("Unreachable: must be already filtered in InputPath.IsIncluded")
        };

        if (path is ['w', 'w', 'w', 'r', 'o', 'o', 't', '/', ..])
            path = path[8..]; // skip "wwwroot/"

        if (path is ['/', ..])
            return $"{path.ToString()}.js";
        else
            return $"/{path.ToString()}.js";
    }

    /// <summary>
    /// Retrieves the file name of the path (without ".razor") and replaces unsafe characters with "_".
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    protected string CreateModuleName(ReadOnlySpan<char> path) {
        // FileName
        int lastSlash = path.LastIndexOf('/');
        ReadOnlySpan<char> rawModuleName = (lastSlash != -1) switch {
            true => path[(lastSlash + 1)..],
            false => path
        };

        if (rawModuleName is [.., '.', 'r', 'a', 'z', 'o', 'r'])
            rawModuleName = rawModuleName[..^6]; // skip ".razor"

        if (rawModuleName.Length > 0) {
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

            return saveModuleName.ToString();
        }
        else
            return string.Empty;
    }


    #region IEquatable

    public static bool operator ==(TSFile left, TSFile right) => left.Equals(right);

    public static bool operator !=(TSFile left, TSFile right) => !left.Equals(right);

    public override bool Equals(object obj)
        => obj switch {
            TSFile other => Equals(other),
            _ => false
        };

    public bool Equals(TSFile other) {
        if (FilePath != other.FilePath)
            return false;

        if (URLPath != other.URLPath)
            return false;

        if (Name != other.Name)
            return false;

        if (!FunctionList.SequenceEqual(other.FunctionList))
            return false;

        return true;
    }

    public override int GetHashCode() {
        int hash = FilePath.GetHashCode();
        hash = Combine(hash, URLPath.GetHashCode());
        hash = Combine(hash, Name.GetHashCode());

        foreach (TSFunction tsFunction in FunctionList)
            hash = Combine(hash, tsFunction.GetHashCode());

        return hash;


        static int Combine(int h1, int h2) {
            uint r = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)r + h1 ^ h2;
        }
    }

    #endregion
}
