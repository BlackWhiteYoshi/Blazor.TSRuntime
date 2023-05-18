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
    public bool TypeNullable { get; set; } = false;

    /// <summary>
    /// Indicates if the given parameter is an array.
    /// </summary>
    public bool Array { get; set; } = false;

    /// <summary>
    /// Indicates if the array itself may be null.
    /// </summary>
    public bool ArrayNullable { get; set; } = false;

    /// <summary>
    /// Indicates if the parameter is optional
    /// </summary>
    public bool Optional { get; set; } = false;


    /// <summary>
    /// <para>Parses the name of the given subStr.</para>
    /// <para>Currently ignoring an optional question mark.</para>
    /// </summary>
    /// <param name="subStr"></param>
    public void ParseName(ReadOnlySpan<char> subStr) {
        if (subStr is [.., '?']) {
            Optional = true;
            Name = subStr[..^1].ToString();
        }
        else
            Name = subStr.ToString();
    }

    /// <summary>
    /// <para>Parses the type of the given subStr.</para>
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
        if (subStr.StartsWith("readonly ".AsSpan()))
            subStr = subStr[9..];

        {
            (TypeNullable, bool optional) = ParseNullUndefined(ref subStr);
            Optional |= optional;
        }

        if (subStr.EndsWith("[]".AsSpan())) {
            Array = true;
            ArrayNullable = TypeNullable;
            subStr = subStr[..^2];   // cut "..[]"
            if (subStr[0] == '(') {
                subStr = subStr[1..^1]; // cut "(..)"
                (bool nullable, bool optional) = ParseNullUndefined(ref subStr);
                TypeNullable = nullable | optional;
            }
            else
                TypeNullable = false;
        }
        else if (subStr.StartsWith("Array<".AsSpan())) {
            Array = true;
            ArrayNullable = TypeNullable;
            subStr = subStr[6..^1];   // cut "Array<..>"
            (bool nullable, bool optional) = ParseNullUndefined(ref subStr);
            TypeNullable = nullable | optional;
        }

        Type = subStr.ToString();
        return;


        static (bool nullable, bool optional) ParseNullUndefined(ref ReadOnlySpan<char> subStr) {
            switch (subStr) {
                case [.., '|', ' ', 'n', 'u', 'l', 'l', ' ', '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd']
                or [.., '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', ' ', 'n', 'u', 'l', 'l']:
                    subStr = subStr[..^19]; // cut " | null | undefined" or " | undefined | null"
                    return (true, true);
                case ['n', 'u', 'l', 'l', ' ', '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', ..]
                or ['u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', ' ', 'n', 'u', 'l', 'l', ' ', '|', ..]:
                    subStr = subStr[19..]; // cut "null | undefined | " or "undefined | null | "
                    return (true, true);
                case ['n', 'u', 'l', 'l', ' ', '|', .., '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd']:
                    subStr = subStr[7..^12]; // cut "null | .. | undefined"
                    return (true, true);
                case ['u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', .., '|', ' ', 'n', 'u', 'l', 'l']:
                    subStr = subStr[12..^7]; // cut "undefined | .. | null"
                    return (true, true);

                case [.., '|', ' ', 'n', 'u', 'l', 'l']:
                    subStr = subStr[..^7]; // cut " | null"
                    return (true, false);
                case ['n', 'u', 'l', 'l', ' ', '|', ..]:
                    subStr = subStr[7..]; // cut "null | "
                    return (true, false);
                case [.., '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd']:
                    subStr = subStr[..^12]; // cut " | undefined"
                    return (false, true);
                case ['u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', ..]:
                    subStr = subStr[12..]; // cut "undefined | "
                    return (false, true);
            }

            return (false, false);
        }
    }
}
