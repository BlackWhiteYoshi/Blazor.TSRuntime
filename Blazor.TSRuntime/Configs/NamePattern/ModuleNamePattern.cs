using Microsoft.CodeAnalysis;
using System.Text;

namespace TSRuntime.Configs.NamePattern;

/// <summary>
/// Naming with 1 variable: #module#.
/// </summary>
public readonly struct ModuleNamePattern : IEquatable<ModuleNamePattern> {
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
    /// <para>placeholder:<br />#module#</para>
    /// </param>
    /// <param name="moduleTransform">Upper/Lower case transform for the #module# placeholder.</param>
    /// <param name="errorList"></param>
    public ModuleNamePattern(string namePattern, NameTransform moduleTransform, List<Diagnostic> errorList) {
        NamePattern = namePattern;
        ModuleTransform = moduleTransform;


        ReadOnlySpan<char> str = namePattern.AsSpan();

        while (str.Length > 0) {
            // first '#'
            int index = str.IndexOf('#');

            // has no "#"
            if (index == -1) {
                if (str.Length > 0)
                    outputList.Add(str.ToString());
                return;
            }

            // read in [..#]
            if (index > 0) {
                outputList.Add(str[..index].ToString());
                str = str[index..];
            }


            // second '#'
            index = str[1..].IndexOf('#') + 1;

            // has no second '#'
            if (index == 0) {
                errorList.AddConfigNamePatternMissingEndTagError();
                return;
            }

            // read in [#..#]
            int length = index + 1;
            if (str[..length] is ['#', 'm', 'o', 'd', 'u', 'l', 'e', '#'])
                outputList.Add(Output.Module);
            else
                errorList.AddConfigNamePatternInvalidVariableError(str[1..index].ToString(), ["module"]);


            str = str[length..];
        }
    }

    /// <summary>
    /// Appends the name based on the values of this object and the given parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="module">Name of the module.</param>
    /// <returns></returns>
    public readonly void AppendNaming(StringBuilder builder, string module) {
        string moduleName = ModuleTransform.Transform(module);

        foreach (OutputBlock block in outputList)
            builder.Append(block.Output switch {
                Output.Module => moduleName,
                Output.String => block.Content,
                _ => throw new Exception("not reachable")
            });
    }


    #region IEquatable

    public static bool operator ==(ModuleNamePattern left, ModuleNamePattern right) => left.Equals(right);

    public static bool operator !=(ModuleNamePattern left, ModuleNamePattern right) => !left.Equals(right);

    public override bool Equals(object obj)
        => obj switch {
            ModuleNamePattern other => Equals(other),
            _ => false
        };

    public bool Equals(ModuleNamePattern other) {
        if (NamePattern != other.NamePattern)
            return false;

        if (ModuleTransform != other.ModuleTransform)
            return false;

        return true;
    }

    public override int GetHashCode() {
        int hashCode = NamePattern.GetHashCode();

        hashCode = Combine(hashCode, ModuleTransform.GetHashCode());

        return hashCode;

        static int Combine(int h1, int h2) {
            uint r = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)r + h1 ^ h2;
        }
    }

    #endregion
}
