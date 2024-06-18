using Microsoft.CodeAnalysis;

namespace TSRuntime.Parsing;

/// <summary>
/// Represents a js-script (a js-file placed in html).
/// </summary>
public sealed class TSScript : TSFile {
    /// <summary>
    /// Creates an object with <see cref="TSFile.FilePath"/>, <see cref="TSFile.URLPath"/> and <see cref="TSFile.Name"/> filled and an empty <see cref="TSFile.FunctionList"/>.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="errorList"></param>
    public TSScript(string filePath, List<Diagnostic> errorList) {
        FilePath = filePath;

        // ModulePath
        ReadOnlySpan<char> path = filePath.AsSpan();
        URLPath = CreateURLPath(ref path);

        Name = CreateModuleName(path);
    }

    /// <summary>
    /// Creates an object with FunctionList.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="urlPath"></param>
    /// <param name="name"></param>
    /// <param name="functionList"></param>
    public TSScript(string filePath, string urlPath, string name, IReadOnlyList<TSFunction> functionList) => (FilePath, URLPath, Name, FunctionList) = (filePath, urlPath, name, functionList);
}
