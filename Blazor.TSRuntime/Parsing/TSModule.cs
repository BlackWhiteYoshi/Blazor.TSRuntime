using Microsoft.CodeAnalysis;
using TSRuntime.Configs;

namespace TSRuntime.Parsing;

/// <summary>
/// Represents a js-module (a js-file loaded as module).
/// </summary>
public sealed class TSModule : IEquatable<TSModule> {
    /// <summary>
    /// The raw given filePath to the module.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The <see cref="FilePath"/> but it is relative, starts with "/" and ends with ".js", also ignoring starting "/wwwroot".
    /// </summary>
    public string URLPath { get; }

    /// <summary>
    /// fileName without ending ".d.ts" or ".razor" and not allowed variable-characters are replaced with '_'.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// List of js-functions of a module (js-file).
    /// </summary>
    public IReadOnlyList<TSFunction> FunctionList { get; } = [];


    /// <summary>
    /// Creates an object with <see cref="FilePath"/>, <see cref="URLPath"/> and <see cref="Name"/> filled and an empty <see cref="FunctionList"/>.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="modulePath"></param>
    /// <param name="errorList"></param>
    public TSModule(string filePath, string? modulePath, List<Diagnostic> errorList) {
        FilePath = filePath;

        // ModulePath
        ReadOnlySpan<char> path;
        if (modulePath == null) {
            path = filePath.AsSpan();

            path = path switch {
                [.., '.', 'j', 's'] => path[..^3], // skip ".js"
                [.., '.', 'd', '.', 't', 's'] => path[..^5], // skip ".d.ts"
                _ => throw new Exception("Unreachable: must be already filtered in InputPath.IsIncluded")
            };

            if (path is ['w', 'w', 'w', 'r', 'o', 'o', 't', '/', ..])
                path = path[8..]; // skip "wwwroot/"

            if (path is ['/', ..])
                URLPath = $"{path.ToString()}.js";
            else
                URLPath = $"/{path.ToString()}.js";
        }
        else {
            if (!modulePath.EndsWith(".js"))
                errorList.AddModulePathNoJsExtensionError(filePath, modulePath);

            int startIndex;
            if (modulePath is ['/', ..]) {
                URLPath = modulePath;
                startIndex = 1;
            }
            else {
                URLPath = $"/{modulePath}";
                startIndex = 0;
            }
            int extensionIndex = modulePath.LastIndexOf('.');
            if (extensionIndex != -1)
                path = modulePath.AsSpan(startIndex, extensionIndex - startIndex);
            else
                path = modulePath.AsSpan(startIndex);
        }

        // ModuleName - Retrieves the file name of the path (without ".razor") and replaces unsafe characters with "_".
        {
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

                Name = saveModuleName.ToString();
            }
            else
                Name = string.Empty;
        }
    }


    private TSModule(string filePath, string urlPath, string name, IReadOnlyList<TSFunction> functionList) => (FilePath, URLPath, Name, FunctionList) = (filePath, urlPath, name, functionList);

    /// <summary>
    /// <para>Parses the file given in <see cref="FilePath"/> and adds the found functions in <see cref="FunctionList"/></para>
    /// <para><see cref="FunctionList"/> is cleared before adding some functions.</para>
    /// </summary>
    /// <param name="fileContent"></param>
    /// <param name="config">only used for appending errors.</param>
    public TSModule ParseFunctions(string fileContent, Config config) {
        List<TSFunction> functionList = [];
        
        int lineNumber = 0;
        int lineStart = 0;
        while (lineStart < fileContent.Length) {
            int lineEnd = fileContent.IndexOf('\n', lineStart);
            if (lineEnd == -1)
                lineEnd = fileContent.Length;
            ReadOnlySpan<char> line = fileContent.AsSpan(lineStart, lineEnd - lineStart).Trim();
            bool isJsFile = FilePath is [.., '.', 'j', 's']; // .js or .d.ts, already filtered at InputPath.IsIncluded
            lineNumber++;

            TSFunction? tsFunction = isJsFile ? TSFunction.ParseJSFunction(line) : TSFunction.ParseTSFunction(line);
            if (tsFunction is not null)
                if (tsFunction.Error.descriptor is null) {
                    tsFunction.ParseSummary(fileContent, lineStart, isJsFile);
                    functionList.Add(tsFunction);
                }
                else
                    config.ErrorList.AddFunctionParseError(tsFunction.Error.descriptor, FilePath, lineNumber, tsFunction.Error.position);

            lineStart = lineEnd + 1;
        }

        return new TSModule(FilePath, URLPath, Name, functionList);
    }


    #region IEquatable

    public static bool operator ==(TSModule left, TSModule right) => left.Equals(right);

    public static bool operator !=(TSModule left, TSModule right) => !left.Equals(right);

    public override bool Equals(object obj)
        => obj switch {
            TSModule other => Equals(other),
            _ => false
        };

    public bool Equals(TSModule other) {
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
