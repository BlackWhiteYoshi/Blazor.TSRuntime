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
        bool nullable;
        Range newRange;

        (nullable, newRange) = ParseNullable(subStr);
        if (nullable)
            subStr = subStr[newRange];

        if (subStr.EndsWith("[]".AsSpan())) {
            Array = true;
            ArrayNullable = nullable;
            subStr = subStr[..^2];   // cut "..[]"
            if (subStr[0] == '(') {
                subStr = subStr[1..^1]; // cut "(..)"
                (nullable, newRange) = ParseNullable(subStr);
                if (nullable)
                    subStr = subStr[newRange];
            }
            else
                nullable = false;
        }
        else if (subStr.StartsWith("Array<".AsSpan())) {
            Array = true;
            ArrayNullable = nullable;
            subStr = subStr[6..^1];   // cut "Array<..>"
            (nullable, newRange) = ParseNullable(subStr);
            if (nullable)
                subStr = subStr[newRange];
        }

        TypeNullable = nullable;
        Type = subStr.ToString();
        return;


        static (bool nullable, Range newRange) ParseNullable(ReadOnlySpan<char> subStr) {
            if (subStr.StartsWith("null |".AsSpan()))
                return (true, new Range(Index.FromStart(7), 0));    // cut "null | .."
            
            if (subStr.StartsWith("undefined |".AsSpan()))
                return (true, new Range(Index.FromStart(12), 0));   // cut "undefined | .."

            if (subStr.EndsWith("| null".AsSpan()))
                return (true, new Range(0, Index.FromEnd(7)));    // cut "..| null"
            
            if (subStr.EndsWith("| undefined".AsSpan()))
                return (true, new Range(0, Index.FromEnd(12)));   // cut "..| undefined"

            return (false, default);
        }
    }
}
