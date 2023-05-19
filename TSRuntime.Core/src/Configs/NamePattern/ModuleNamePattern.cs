namespace TSRuntime.Core.Configs.NamePattern;

/// <summary>
/// Naming with 1 variable: #module#.
/// </summary>
public struct ModuleNamePattern : IEquatable<ModuleNamePattern>
{
    private const string MODULE = "#module#";

    
    private readonly List<OutputBlock> outputList = new(3); // default "I#module#Module" are 3 entries
    /// <summary>
    /// <para>The name pattern for creating the name.</para>
    /// <para>
    /// placeholder:<br />
    /// #module#
    /// </para>
    /// </summary>
    public string NamePattern { get; }
    /// <summary>
    /// Upper/Lower case transform for the #module# placeholder.
    /// </summary>
    public NameTransform ModuleTransform { get; }


    /// <summary>
    /// Parses the given namePattern to construct an <see cref="outputList">outputList</see>.
    /// </summary>
    /// <param name="namePattern">
    /// <para>The name pattern for creating the method name.</para>
    /// <para>placeholder:<br />
    /// #module#</para></param>
    /// <param name="moduleTransform">Upper/Lower case transform for the #module# placeholder.</param>
    /// <exception cref="ArgumentException">Throws when an invalid placeholder in namePattern is used e.g. #invalid#</exception>
    public ModuleNamePattern(string namePattern, NameTransform moduleTransform)
    {
        NamePattern = namePattern;
        ModuleTransform = moduleTransform;


        ReadOnlySpan<char> str = namePattern.AsSpan();

        while (str.Length > 0)
        {
            int index = str.IndexOf('#');

            // has no "#"
            if (index == -1)
            {
                outputList.Add(str.ToString());
                return;
            }

            // read in ..#
            if (index > 0)
            {
                outputList.Add(str[..index].ToString());
                str = str[index..];
            }

            // read in #module#
            if (str.StartsWith(MODULE.AsSpan())) {
                outputList.Add(Output.Module);
                str = str[MODULE.Length..];
            }
            else
               throw new ArgumentException($"Only argument {MODULE} is allowed");
        }
    }

    /// <summary>
    /// Returns the name based on the values of this object and the given parameters.
    /// </summary>
    /// <param name="module">Name of the module.</param>
    /// <returns></returns>
    /// <exception cref="Exception">Throws when this object is created with an invalid enum value.</exception>
    public readonly IEnumerable<string> GetNaming(string module)
    {
        string moduleName = ModuleTransform.Transform(module);


        foreach (OutputBlock block in outputList)
            yield return block.output switch
            {
                Output.Module => moduleName,
                Output.String => block.content,
                _ => throw new Exception("not reachable")
            };
    }


    #region IEquatable

    public readonly bool Equals(ModuleNamePattern other) {
        if (NamePattern != other.NamePattern)
            return false;

        if (ModuleTransform != other.ModuleTransform)
            return false;

        return true;
    }

    public override readonly bool Equals(object obj) {
        if (obj is not ModuleNamePattern other)
            return false;

        return Equals(other);
    }

    public static bool operator ==(ModuleNamePattern left, ModuleNamePattern right) {
        return left.Equals(right);
    }

    public static bool operator !=(ModuleNamePattern left, ModuleNamePattern right) {
        return !left.Equals(right);
    }

    public override readonly int GetHashCode() {
        return Combine(NamePattern.GetHashCode(), ModuleTransform.GetHashCode());


        static int Combine(int h1, int h2) {
            uint r = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)r + h1) ^ h2;
        }
    }

    #endregion
}
