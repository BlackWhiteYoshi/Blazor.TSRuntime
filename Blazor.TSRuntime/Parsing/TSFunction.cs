namespace TSRuntime.Parsing;

/// <summary>
/// Represents a js-function inside a <see cref="TSModule"/>.
/// </summary>
public sealed class TSFunction : IEquatable<TSFunction> {
    /// <summary>
    /// The name of the js-function. 
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// List of the parameters of this js-function.
    /// </summary>
    public List<TSParameter> ParameterList { get; } = [];

    /// <summary>
    /// <para>Holds information about the return value.</para>
    /// <para>Since return values have no names, the <see cref="TSParameter.Name"/> defaults to "ReturnValue".</para>
    /// </summary>
    public TSParameter ReturnType { get; } = new TSParameter("ReturnValue");
    
    /// <summary>
    /// Indicates if the <see cref="ReturnType"/> is a promise or not.
    /// </summary>
    public bool ReturnPromise { get; private set; }


    /// <summary>
    /// Creates a TSFunction if the given line represents a exported js-function. 
    /// </summary>
    /// <param name="line">An Entire line in a "d.ts"-file.</param>
    /// <returns>null, if not starting with "export declare function ", otherwise tries to parse and returns a <see cref="TSFunction"/>.</returns>
    public static TSFunction? Parse(ReadOnlySpan<char> line) {
        if (line.StartsWith("export declare function ".AsSpan()))
            line = line[24..]; // skip "export declare function "
        else if (line.StartsWith("export function ".AsSpan()))
            line = line[16..]; // skip "export function "
        else
            return null;

        TSFunction tsFunction = new();

        // FunctionName
        int openBracket = line.IndexOf('(');
        if (openBracket == -1)
            return null; // new Exception($"invalid d.ts file: '{c}' expected");
        tsFunction.Name = line[..openBracket].ToString();

        line = line[(openBracket + 1)..]; // skip "("

        // Parameters
        tsFunction.ParameterList.Clear();
        if (line[0] == ')')
            line = line[3..]; // no parameters, skip "): "
        else
            while (true) {
                // parameter
                TSParameter tsParameter = new();
                tsFunction.ParameterList.Add(tsParameter);
                
                // parse Name
                int colon = line.IndexOf(':');
                if (colon == -1)
                    return null; // new Exception($"invalid d.ts file: '{c}' expected");
                tsParameter.ParseName(line[..colon]);
                line = line[(colon + 2)..]; // skip ": "

                // parse Type
                int parameterTypeEnd;
                int bracketCount = 0;
                for (int i = 0; i < line.Length; i++) {
                    char c = line[i];
                    switch (c) {
                        case ',':
                            parameterTypeEnd = i;
                            goto brackets_counted;
                        case '(':
                            bracketCount++;
                            break;
                        case ')':
                            if (bracketCount == 0) {
                                parameterTypeEnd = i;
                                goto brackets_counted;
                            }
                            bracketCount--;
                            break;
                    }
                }
                // else
                {
                    return null; // new Exception($"invalid d.ts file: no end of parameter found, expected ',' or ')'");
                }
                brackets_counted:

                //int parameterTypeEnd = IndexOfParameterEnd(line);
                tsParameter.ParseType(line[..parameterTypeEnd]);
                line = line[parameterTypeEnd..];

                if (line[0] == ',')
                    line = line[2..]; // skip ", "
                else {
                    line = line[3..]; // no parameters, skip "): "
                    break;
                }
            }

        // ReturnType/Promise
        int semicolon = line.IndexOf(';');
        if (semicolon == -1)
            return null;
        if (line.StartsWith("Promise<".AsSpan())) {
            tsFunction.ReturnPromise = true;
            line = line[8..(semicolon - 1)]; // cut "Promise<..>"
        }
        else {
            tsFunction.ReturnPromise = false;
            line = line[..semicolon];
        }
        tsFunction.ReturnType.ParseType(line);

        return tsFunction;
    }


    #region IEquatable

    public static bool operator ==(TSFunction left, TSFunction right) => left.Equals(right);

    public static bool operator !=(TSFunction left, TSFunction right) => !left.Equals(right);

    public override bool Equals(object obj)
        => obj switch {
            TSFunction other => Equals(other),
            _ => false
        };

    public bool Equals(TSFunction other) {
        if (Name != other.Name)
            return false;

        if (!ParameterList.SequenceEqual(other.ParameterList))
            return false;

        if (ReturnType != other.ReturnType)
            return false;

        if (ReturnPromise != other.ReturnPromise)
            return false;

        return true;
    }

    public override int GetHashCode() {
        int hash = Name.GetHashCode();

        foreach (TSParameter tsParameter in ParameterList)
            hash = Combine(hash, tsParameter.GetHashCode());

        hash = Combine(hash, ReturnType.GetHashCode());
        hash = Combine(hash, ReturnPromise.GetHashCode());

        return hash;


        static int Combine(int h1, int h2) {
            uint r = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)r + h1 ^ h2;
        }
    }

    #endregion
}
