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
    /// <para>The js-type of the parameter/array.</para>
    /// <para>If null, <see cref="typeCallback"/> should be used and <see cref="typeCallback"/> holds at least one item.</para>
    /// </summary>
    public string? type = string.Empty;

    /// <summary>
    /// <para>The js-type of the parameter when it is a callback.</para>
    /// <para>The last item is the returnType.</para>
    /// <para>If empty, <see cref="type"/> should be used.</para>
    /// </summary>
    public TSParameter[] typeCallback = [];

    /// <summary>
    /// If this parameter is a callback (if <see cref="type"/> is null), this value indicates if the returnType of that callback is a Promise.
    /// </summary>
    public bool typeCallbackPromise = false;

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

        int arrowIndex;
        {
            arrowIndex = -1;
            int bracketCount = 0;
            for (int i = 0; i < subStr.Length - 1; i++)
                switch (subStr[i]) {
                    case '(':
                        bracketCount++;
                        break;
                    case ')':
                        bracketCount--;
                        break;
                    case '=':
                        if (bracketCount == 0)
                            if (subStr[i + 1] is '>') {
                                arrowIndex = i;
                                goto arrowIndex_double_break;
                            }
                        break;
                }
        }
        arrowIndex_double_break:

        if (arrowIndex != -1) {
            type = null;

            ReadOnlySpan<char> parameterStr = subStr[..arrowIndex].TrimEnd();

            if (parameterStr is not ['(', .., ')'])
                return;
            parameterStr = parameterStr[1..^1].Trim(); // cut "(..)"


            List<TSParameter> parameterList = [];

            // arrow function parameters
            while (parameterStr.Length > 0) {
                TSParameter tsParameter = new();

                // parse name
                int colonIndex = parameterStr.IndexOfAny([':']);
                if (colonIndex != -1) {
                    tsParameter.ParseName(parameterStr[..colonIndex].TrimEnd());
                    parameterStr = parameterStr[(colonIndex + 1)..].TrimStart();
                }

                // parse type
                int parameterTypeEnd;
                {
                    int bracketCount = 0;
                    int i;
                    for (i = 0; i < parameterStr.Length; i++)
                        switch (parameterStr[i]) {
                            case '(' or '[' or '<':
                                bracketCount++;
                                break;
                            case ')' or ']' or '>':
                                bracketCount--;
                                break;
                            case ',':
                                if (bracketCount <= 0)
                                    goto double_break;
                                break;
                        }
                    double_break:
                    parameterTypeEnd = i;
                }

                tsParameter.ParseType(parameterStr[..parameterTypeEnd].TrimEnd());
                if (parameterTypeEnd < parameterStr.Length)
                    parameterStr = parameterStr[(parameterTypeEnd + 1)..];
                else
                    parameterStr = [];

                parameterList.Add(tsParameter);
            }

            // arrow function returnType
            TSParameter returnType = new() { name = "ReturnValue", type = "void" };
            ReadOnlySpan<char> returnStr = subStr[(arrowIndex + 2)..].TrimStart(); // skip "=>"

            typeCallbackPromise = returnStr is ['P', 'r', 'o', 'm', 'i', 's', 'e', '<', ..];
            if (typeCallbackPromise) {
                int closingBracket = returnStr.LastIndexOf('>');
                if (closingBracket != -1)
                    returnStr = returnStr[8..closingBracket].Trim();
                else
                    returnStr = returnStr[8..].Trim();
            }
            returnType.ParseType(returnStr);


            typeCallback = [.. parameterList, returnType];
        }
        else {
            // array or type
            (typeNullable, bool isOptional) = ParseNullUndefined(ref subStr);
            optional |= isOptional;

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
}
