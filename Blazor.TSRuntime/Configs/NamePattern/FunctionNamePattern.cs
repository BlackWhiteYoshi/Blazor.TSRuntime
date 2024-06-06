using Microsoft.CodeAnalysis;
using System.Text;

namespace TSRuntime.Configs.NamePattern;

/// <summary>
/// <para>Naming of the generated methods that invoke js-functions.</para>
/// <para>It supports the variables #function#, #module# and #action#.</para>
/// </summary>
public readonly struct FunctionNamePattern : IEquatable<FunctionNamePattern> {
    private const string MODULE = "#module#";
    private const string FUNCTION = "#function#";
    private const string ACTION = "#action#";


    private readonly List<OutputBlock> outputList = new(1); // default "#function#" is 1 entry
    /// <summary>
    /// <para>The name pattern for creating the method name.</para>
    /// <para>placeholder:<br />
    /// #function#<br />
    /// #module#<br />
    /// #action#</para>
    /// </summary>
    public string NamePattern { get; }
    /// <summary>
    /// Upper/Lower case transform for the #module# placeholder.
    /// </summary>
    public NameTransform ModuleTransform { get; }
    /// <summary>
    /// Upper/Lower case transform for the #function# placeholder.
    /// </summary>
    public NameTransform FunctionTransform { get; }
    /// <summary>
    /// Upper/Lower case transform for the #action# placeholder.
    /// </summary>
    public NameTransform ActionTransform { get; }


    /// <summary>
    /// Parses the given namePattern to construct an <see cref="outputList">outputList</see>.
    /// </summary>
    /// <param name="namePattern">
    /// <para>The name pattern for creating the method name.</para>
    /// <para>placeholder:<br />
    /// #function#<br />
    /// #module#<br />
    /// #action#</para>
    /// </param>
    /// <param name="moduleTransform">Upper/Lower case transform for the #module# placeholder.</param>
    /// <param name="functionTransform">Upper/Lower case transform for the #function# placeholder.</param>
    /// <param name="actionTransform">Upper/Lower case transform for the #action# placeholder.</param>
    /// <param name="errorList">Is only used when input is invalud.</param>
    public FunctionNamePattern(string namePattern, NameTransform moduleTransform, NameTransform functionTransform, NameTransform actionTransform, List<Diagnostic> errorList) {
        NamePattern = namePattern;
        ModuleTransform = moduleTransform;
        FunctionTransform = functionTransform;
        ActionTransform = actionTransform;


        ReadOnlySpan<char> str = namePattern.AsSpan();

        while (str.Length > 0) {
            // first '#'
            int index = str.IndexOf('#');
            
            // has no '#'
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
            switch (length) {
                case 8: // "#module#" or "#action#"
                    if (str.StartsWith(MODULE.AsSpan()))
                        outputList.Add(Output.Module);
                    else if (str.StartsWith(ACTION.AsSpan()))
                        outputList.Add(Output.Action);
                    else
                        goto default;
                    break;
                case 10: // "#function#"
                    if (str.StartsWith(FUNCTION.AsSpan()))
                        outputList.Add(Output.Function);
                    else
                        goto default;
                    break;
                default:
                    errorList.AddConfigNamePatternInvalidVariableError(str[1..(length - 1)].ToString(), ["module", "function", "action"]);
                    break;
            }


            str = str[length..];
        }
    }

    /// <summary>
    /// Appends the name of the method based on the values of this object and the given parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="module">Name of the module.</param>
    /// <param name="function">Name of the function.</param>
    /// <param name="action">Name of the action.</param>
    /// <returns></returns>
    public readonly void AppendNaming(StringBuilder builder, string module, string function, string action) {
        string moduleName = ModuleTransform.Transform(module);
        string functionName = FunctionTransform.Transform(function);
        string actionName = ActionTransform.Transform(action);

        foreach (OutputBlock block in outputList)
            builder.Append(block.Output switch {
                Output.Module => moduleName,
                Output.Function => functionName,
                Output.Action => actionName,
                Output.String => block.Content,
                _ => throw new Exception("not reachable")
            });
    }


    #region IEquatable

    public static bool operator ==(FunctionNamePattern left, FunctionNamePattern right) => left.Equals(right);

    public static bool operator !=(FunctionNamePattern left, FunctionNamePattern right) => !left.Equals(right);

    public override bool Equals(object obj)
        => obj switch {
            FunctionNamePattern other => Equals(other),
            _ => false
        };

    public bool Equals(FunctionNamePattern other) {
        if (NamePattern != other.NamePattern)
            return false;

        if (ModuleTransform != other.ModuleTransform)
            return false;
        if (FunctionTransform != other.FunctionTransform)
            return false;
        if (ActionTransform != other.ActionTransform)
            return false;

        return true;
    }

    public override int GetHashCode() {
        int hashCode = NamePattern.GetHashCode();

        hashCode = Combine(hashCode, ModuleTransform.GetHashCode());
        hashCode = Combine(hashCode, FunctionTransform.GetHashCode());
        hashCode = Combine(hashCode, ActionTransform.GetHashCode());

        return hashCode;

        static int Combine(int h1, int h2) {
            uint r = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)r + h1 ^ h2;
        }
    }

    #endregion
}
