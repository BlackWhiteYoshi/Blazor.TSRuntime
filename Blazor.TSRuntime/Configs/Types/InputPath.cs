namespace TSRuntime.Configs;

public readonly struct InputPath(string include, string[] excludes) : IEquatable<InputPath> {
    /// <summary>
    /// <para>Path to a folder. Every .d.ts-file in that folder will be included.</para>
    /// <para>It can also be a path to a file. If this is a file-path, <see cref="ModulePath"/> must also be set.</para>
    /// <para>No trailing slash.</para>
    /// </summary>
    public string Include { get; init; } = include;

    /// <summary>
    /// <para>Excludes specific folders or files from <see cref="Include"/>.</para>
    /// <para>
    /// Every path must start with the path given in <see cref="Include"/>, otherwise that path won't match.<br />
    /// No trailing slash allowed, otherwise that path won't match.
    /// </para>
    /// </summary>
    public string[] Excludes { get; init; } = excludes;

    /// <summary>
    /// Indicates if the files in located at this path are modules or scripts.
    /// </summary>
    public bool ModuleFiles { get; init; } = true;

    /// <summary>
    /// <para>
    /// Relative Path/URL to load the module.<br />
    /// e.g. "Pages/Footer/Contacts.razor.js"
    /// </para>
    /// <para>If <see cref="Include"/> is a folder path, this does nothing.</para>
    /// </summary>
    public string? ModulePath { get; init; } = null;


    /// <summary>
    /// Sets <see cref="Include"/> to given string and <see cref="Excludes"/> to an empty array.
    /// </summary>
    /// <param name="include"></param>
    public InputPath(string include) : this(include, []) { }

    /// <summary>
    /// Sets all parameters of this data structure.
    /// </summary>
    /// <param name="include"></param>
    /// <param name="excludes"></param>
    /// <param name="moduleFiles"></param>
    /// <param name="modulePath"></param>
    public InputPath(string include, string[] excludes, bool moduleFiles, string? modulePath) : this(include, excludes) {
        ModuleFiles = moduleFiles;
        ModulePath = modulePath;
    }


    public void Deconstruct(out string include, out string[] excludes, out bool moduleFiles, out string? modulePath) {
        include = Include;
        excludes = Excludes;
        moduleFiles = ModuleFiles;
        modulePath = ModulePath;
    }


    /// <summary>
    /// <para>Checks if the filePath is not in the given exclude list.</para>
    /// <para>filePath and excludes must start with the same characters.<br />
    /// exclude paths must not end with trailing slash.</para>
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool IsIncluded(string filePath) {
        if (!filePath.StartsWith(Include) || filePath is not ([.., '.', 'j', 's'] or [.., '.', 't', 's'])) // ".d.ts" ends with ".ts"
            return false;

        foreach (string exclude in Excludes) {
            if (exclude.Length == 0)
                return false;

            if (filePath.StartsWith(exclude)) {
                // exclude is file
                if (filePath.Length == exclude.Length)
                    return false;

                // exclude is folder
                if (filePath[exclude.Length] == '/')
                    return false;
            }
        }

        return true;
    }


    #region IEquatable

    public static bool operator ==(InputPath left, InputPath right) => left.Equals(right);

    public static bool operator !=(InputPath left, InputPath right) => !left.Equals(right);

    public override bool Equals(object obj)
        => obj switch {
            InputPath other => Equals(other),
            _ => false
        };

    public bool Equals(InputPath other) {
        if (Include != other.Include)
            return false;

        if (!Excludes.SequenceEqual(other.Excludes))
            return false;

        if (!ModuleFiles != other.ModuleFiles)
            return false;

        if (ModulePath != other.ModulePath)
            return false;

        return true;
    }

    public override int GetHashCode() {
        int hash = Include.GetHashCode();

        foreach (string exclude in Excludes)
            hash = Combine(hash, exclude.GetHashCode());

        hash = Combine(hash, ModuleFiles.GetHashCode());

        hash = Combine(hash, ModulePath?.GetHashCode() ?? 0);

        return hash;


        static int Combine(int h1, int h2) {
            uint r = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)r + h1 ^ h2;
        }
    }

    #endregion
}
