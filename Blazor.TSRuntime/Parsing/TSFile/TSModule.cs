using Microsoft.CodeAnalysis;

namespace TSRuntime.Parsing;

/// <summary>
/// Represents a js-module (a js-file loaded as module).
/// </summary>
public sealed class TSModule : TSFile {
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
            URLPath = CreateURLPath(ref path);
        }
        else {
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

        Name = CreateModuleName(path);
    }

    /// <summary>
    /// Creates an object with FunctionList.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="urlPath"></param>
    /// <param name="name"></param>
    /// <param name="functionList"></param>
    public TSModule(string filePath, string urlPath, string name, IReadOnlyList<TSFunction> functionList) => (FilePath, URLPath, Name, FunctionList) = (filePath, urlPath, name, functionList);
}
