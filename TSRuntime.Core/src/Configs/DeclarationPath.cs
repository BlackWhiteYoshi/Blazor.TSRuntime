namespace TSRuntime.Core.Configs;

public struct DeclarationPath : IEquatable<DeclarationPath> {
    /// <summary>
    /// <para>Path to a folder. Every .d.ts-file in that folder will be included.</para>
    /// <para>It can also be a path to a file. If this is a file-path, <see cref="FileModulePath"/> must also be set.</para>
    /// <para>No trailing slash.</para>
    /// </summary>
    public string Include { get; init; }

    /// <summary>
    /// <para>Excludes specific folders or files from <see cref="Include"/>.</para>
    /// <para>
    /// Every path must start with the path given in <see cref="Include"/>, otherwise that path won't match.<br />
    /// No trailing slash allowed, otherwise that path won't match.
    /// </para>
    /// </summary>
    public string[] Excludes { get; init; }

    /// <summary>
    /// <para>Relative Path/URL to load the module.<br />
    /// e.g. "Pages/Footer/Contacts.razor.js"</para>
    /// <para>Must be set if <see cref="Include"/> is a path to a file, otherwise an exception is thrown.<br />
    /// If <see cref="Include"/> is a folder path, this does nothing.</para>
    /// </summary>
    public string? FileModulePath { get; init; }


    /// <summary>
    /// Sets <see cref="Include"/> to given string and <see cref="Excludes"/> to an empty array.
    /// </summary>
    /// <param name="include"></param>
    public DeclarationPath(string include) {
        Include = include;
        Excludes = Array.Empty<string>();
    }

    /// <summary>
    /// Sets <see cref="Include"/> and <see cref="Excludes"/> to the given values.
    /// </summary>
    /// <param name="include"></param>
    /// <param name="excludes"></param>
    public DeclarationPath(string include, string[] excludes) {
        Include = include;
        Excludes = excludes;
    }

    /// <summary>
    /// Sets <see cref="Include"/> and <see cref="FileModulePath"/> to the given values and <see cref="Excludes"/> to an empty array.
    /// </summary>
    /// <param name="include"></param>
    /// <param name="fileModulePath"></param>
    public DeclarationPath(string include, string? fileModulePath) {
        Include = include;
        Excludes = Array.Empty<string>();
        FileModulePath = fileModulePath;
    }

    /// <summary>
    /// Sets <see cref="Include"/>, <see cref="Excludes"/> and <see cref="FileModulePath"/> to the given values.
    /// </summary>
    /// <param name="include"></param>
    /// <param name="excludes"></param>
    /// <param name="fileModulePath"></param>
    public DeclarationPath(string include, string[] excludes, string? fileModulePath) {
        Include = include;
        Excludes = excludes;
        FileModulePath = fileModulePath;
    }


    public void Deconstruct(out string include, out string[] excludes, out string? fileModulePath) {
        include = Include;
        excludes = Excludes;
        fileModulePath = FileModulePath;
    }


    #region IEquatable

    public bool Equals(DeclarationPath other) {
        if (Include != other.Include)
            return false;

        for (int i = 0; i < Excludes.Length; i++)
            if (Excludes[i] != other.Excludes[i])
                return false;

        if (FileModulePath != other.FileModulePath)
            return false;

        return true;
    }

    public override bool Equals(object obj) {
        if (obj is not DeclarationPath other)
            return false;

        return Equals(other);
    }

    public static bool operator ==(DeclarationPath left, DeclarationPath right) {
        return left.Equals(right);
    }

    public static bool operator !=(DeclarationPath left, DeclarationPath right) {
        return !left.Equals(right);
    }

    public override int GetHashCode() {
        int hash = Include.GetHashCode();

        foreach (string exclude in Excludes)
            hash = Combine(hash, exclude.GetHashCode());

        if (FileModulePath != null)
            hash = Combine(hash, FileModulePath.GetHashCode());

        return hash;


        static int Combine(int h1, int h2) {
            uint r = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)r + h1) ^ h2;
        }
    }

    #endregion


    /// <summary>
    /// <para>Checks if the filePath is not in the given exclude list.</para>
    /// <para>filePath and excludes must start with the same characters.<br />
    /// exclude paths must not end with trailing slash.</para>
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="excludes"></param>
    /// <returns></returns>
    public static bool IsIncluded(string filePath, string[] excludes) {
        foreach (string exclude in excludes)
            if (filePath.StartsWith(exclude)) {
                // exclude is file
                if (filePath.Length == exclude.Length)
                    return false;

                // exclude is folder
                if (filePath[exclude.Length] == '/')
                    return false;
            }

        return true;
    }
}
