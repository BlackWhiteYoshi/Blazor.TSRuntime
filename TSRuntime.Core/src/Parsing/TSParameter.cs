namespace TSRuntime.Core.Parsing;

/// <summary>
/// Represents a parameter inside a <see cref="TSFunction"/>.
/// </summary>
public sealed class TSParameter {
    /// <summary>
    /// The given name of the parameter.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The js-type of the parameter/array.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the type may be null.
    /// </summary>
    public bool TypeNullable { get; set; }

    /// <summary>
    /// Indicates if the given parameter is an array.
    /// </summary>
    public bool Array { get; set; }

    /// <summary>
    /// Indicates if the array itself may be null.
    /// </summary>
    public bool ArrayNullable { get; set; }


    /// <summary>
    /// <para>Parses the type of the given parameter.</para>
    /// <para>
    /// e.g.<br />
    /// - number<br />
    /// - number | null<br />
    /// - number | undefined<br />
    /// - number[]<br />
    /// - Array&lt;number&gt;<br />
    /// - (number | null)[]<br />
    /// - (number | null)[] | null
    /// </para>
    /// </summary>
    /// <param name="subStr">Only the part of the string that represents the type of a parameter (starting after ": " and ending before ',' or ')'.</param>
    public void ParseType(ReadOnlySpan<char> subStr) {
        bool nullable = ParseNullable(ref subStr);

        if (subStr.EndsWith("[]".AsSpan())) {
            Array = true;
            ArrayNullable = nullable;
            subStr = subStr[..^2];   // cut "..[]"
            if (subStr[0] == '(') {
                subStr = subStr[1..^1]; // cut "(..)"
                nullable = ParseNullable(ref subStr);
            }
            else
                nullable = false;
        }
        else if (subStr.StartsWith("Array<".AsSpan())) {
            Array = true;
            ArrayNullable = nullable;
            subStr = subStr[6..^1];   // cut "Array<..>"
            nullable = ParseNullable(ref subStr);
        }

        TypeNullable = nullable;
        Type = subStr.ToString();
        return;


        static bool ParseNullable(ref ReadOnlySpan<char> subStr) {
            if (subStr.StartsWith("null |".AsSpan())) {
                subStr = subStr[7..]; // cut "null | .."
                return true;    
            }
            
            if (subStr.StartsWith("undefined |".AsSpan())) {
                subStr = subStr[12..]; // cut "undefined | .."
                return true;   
            }

            if (subStr.EndsWith("| null".AsSpan())) {
                subStr= subStr[..^7]; // cut "..| null"
                return true;
            }
            
            if (subStr.EndsWith("| undefined".AsSpan())) {
                subStr = subStr[..^12]; // cut "..| undefined"
                return true;   
            }

            return false;
        }
    }
}
