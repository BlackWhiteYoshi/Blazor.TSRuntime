namespace TSRuntime.Parsing;

/// <summary>
/// Represents a parameter inside a <see cref="TSFunction"/>.
/// </summary>
public record struct TSParameter() : IEquatable<TSParameter> {
    /// <summary>
    /// Description of this parameter.
    /// </summary>
    public string summary = string.Empty;

    /// <summary>
    /// The given name of the parameter.
    /// </summary>
    public string name = string.Empty;

    /// <summary>
    /// The js-type of the parameter/array.
    /// </summary>
    public string type = string.Empty;

    /// <summary>
    /// Indicates if the type may be null.
    /// </summary>
    public bool typeNullable = false;

    /// <summary>
    /// Indicates if the given parameter is an array.
    /// </summary>
    public bool array = false;

    /// <summary>
    /// Indicates if the array itself may be null.
    /// </summary>
    public bool arrayNullable = false;

    /// <summary>
    /// Indicates if the parameter is optional
    /// </summary>
    public bool optional = false;


    /// <summary>
    /// Parses the name of the given subStr.
    /// </summary>
    /// <param name="subStr"></param>
    public void ParseName(ReadOnlySpan<char> subStr) {
        if (subStr is [.., '?']) {
            optional = true;
            name = subStr[..^1].ToString();
        }
        else
            name = subStr.ToString();
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
        if (subStr is ['r', 'e', 'a', 'd', 'o', 'n', 'l', 'y', ' ', ..]) {
            subStr = subStr[9..];
            subStr = subStr.TrimStart();
        }

        // array or type
        (typeNullable, optional) = ParseNullUndefined(ref subStr);

        if (subStr is [.., ']']) {
            ReadOnlySpan<char> view = subStr[..^1].TrimEnd();
            if (view is [.., '[']) {
                subStr = view[..^1].TrimEnd();
                array = true;
                arrayNullable = typeNullable;

                if (subStr is ['(', .., ')']) {
                    subStr = subStr[1..^1].Trim(); // cut "(..)"
                    (bool nullable, bool optional) = ParseNullUndefined(ref subStr);
                    typeNullable = nullable | optional;
                }
                else
                    typeNullable = false;

                type = subStr.ToString();
                return;
            }
        }
        
        if (subStr is ['A', 'r', 'r', 'a', 'y', '<', .., '>']) {
            array = true;
            arrayNullable = typeNullable;
            subStr = subStr[6..^1].Trim();   // cut "Array<..>"

            (bool nullable, bool optional) = ParseNullUndefined(ref subStr);
            typeNullable = nullable | optional;

            type = subStr.ToString();
            return;
        }
        
        type = subStr.ToString();
        return;


        static (bool nullable, bool optional) ParseNullUndefined(ref ReadOnlySpan<char> subStr) {
            if (IsNullable(ref subStr))
                return (true, IsUndefinedable(ref subStr));
            else if (IsUndefinedable(ref subStr))
                return (IsNullable(ref subStr), true);

            return (false, false);


            static bool IsNullable(ref ReadOnlySpan<char> subStr) {
                if (subStr is ['n', 'u', 'l', 'l', ..]) {
                    ReadOnlySpan<char> view = subStr[4..];
                    view = view.TrimStart();
                    if (view is ['|', ..]) {
                        subStr = view[1..].TrimStart();
                        return true;
                    }
                }

                if (subStr is [.., 'n', 'u', 'l', 'l']) {
                    ReadOnlySpan<char> view = subStr[..^4];
                    view = view.TrimEnd();
                    if (view is [.., '|']) {
                        subStr = view[..^1].TrimEnd();
                        return true;
                    }
                }

                return false;
            }

            static bool IsUndefinedable(ref ReadOnlySpan<char> subStr) {
                if (subStr is ['u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ..]) {
                    ReadOnlySpan<char> view = subStr[9..];
                    view = view.TrimStart();
                    if (view is ['|', ..]) {
                        subStr = view[1..].TrimStart();
                        return true;
                    }
                }

                if (subStr is [.., 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd']) {
                    ReadOnlySpan<char> view = subStr[..^9];
                    view = view.TrimEnd();
                    if (view is [.., '|']) {
                        subStr = view[..^1].TrimEnd();
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
