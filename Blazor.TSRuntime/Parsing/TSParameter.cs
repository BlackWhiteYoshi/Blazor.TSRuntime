namespace TSRuntime.Parsing;

/// <summary>
/// Represents a parameter inside a <see cref="TSFunction"/>.
/// </summary>
public record struct TSParameter() : IEquatable<TSParameter> {
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
    /// <para>Parses the name of the given subStr.</para>
    /// <para>Currently ignoring an optional question mark.</para>
    /// </summary>
    /// <param name="subStr"></param>
    public void ParseName(string line, int start, int end) {
        if (line.Length == 0)
            return;

        if (line[end - 1] == '?') {
            optional = true;
            name = line[start..(end - 1)];
        }
        else
            name = line[start..end];
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
    public void ParseType(string line, int position, int end) {
        if (line.AsSpan(position).StartsWith("readonly ".AsSpan()))
            position += 9;

        // array or type
        {
            (typeNullable, bool optional) = ParseNullUndefined(line, ref position, ref end);
            this.optional = optional;
        }

        if (line[end - 2] == '[' && line[end - 1] == ']') {
            array = true;
            arrayNullable = typeNullable;
            end -= 2;   // cut "..[]"
            if (line[position] == '(') {
                // cut "(..)"
                position += 1;
                end -= 1;

                (bool nullable, bool optional) = ParseNullUndefined(line, ref position, ref end);
                typeNullable = nullable | optional;
            }
            else
                typeNullable = false;
        }
        else if (line.AsSpan(position).StartsWith("Array<".AsSpan())) {
            array = true;
            arrayNullable = typeNullable;
            // cut "Array<..>"
            position += 6;
            end -= 1;

            (bool nullable, bool optional) = ParseNullUndefined(line, ref position, ref end);
            typeNullable = nullable | optional;
        }

        type = line[position..end];
        return;


        static (bool nullable, bool optional) ParseNullUndefined(string line, ref int position, ref int end) {
            switch (line.AsSpan(position, end - position)) {
                case [.., '|', ' ', 'n', 'u', 'l', 'l', ' ', '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd']
                or [.., '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', ' ', 'n', 'u', 'l', 'l']:
                    end -= 19; // cut " | null | undefined" or " | undefined | null"
                    return (true, true);
                case ['n', 'u', 'l', 'l', ' ', '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', ..]
                or ['u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', ' ', 'n', 'u', 'l', 'l', ' ', '|', ..]:
                    position += 19; // cut "null | undefined | " or "undefined | null | "
                    return (true, true);
                case ['n', 'u', 'l', 'l', ' ', '|', .., '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd']:
                    // cut "null | .. | undefined"
                    position += 7;
                    end -= 12;
                    return (true, true);
                case ['u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', .., '|', ' ', 'n', 'u', 'l', 'l']:
                    // cut "undefined | .. | null"
                    position += 12;
                    end -= 7;
                    return (true, true);

                case [.., '|', ' ', 'n', 'u', 'l', 'l']:
                    end -= 7; // cut " | null"
                    return (true, false);
                case ['n', 'u', 'l', 'l', ' ', '|', ..]:
                    position += 7; // cut "null | "
                    return (true, false);
                case [.., '|', ' ', 'u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd']:
                    end -= 12; // cut " | undefined"
                    return (false, true);
                case ['u', 'n', 'd', 'e', 'f', 'i', 'n', 'e', 'd', ' ', '|', ..]:
                    position += 12; // cut "undefined | "
                    return (false, true);
            }

            return (false, false);
        }
    }
}
