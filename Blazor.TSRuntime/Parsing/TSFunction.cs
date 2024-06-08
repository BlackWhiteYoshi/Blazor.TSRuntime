using Microsoft.CodeAnalysis;

namespace TSRuntime.Parsing;

/// <summary>
/// Represents a js-function inside a <see cref="TSModule"/>.
/// </summary>
public sealed class TSFunction : IEquatable<TSFunction> {
    /// <summary>
    /// The name of the js-function. 
    /// </summary>
    public string Name { get; } = string.Empty;

    /// <summary>
    /// List of the generic parameters of this js function
    /// </summary>
    public string[] Generics { get; } = [];

    /// <summary>
    /// List of the parameters of this js-function.
    /// </summary>
    public TSParameter[] ParameterList { get; } = [];

    /// <summary>
    /// <para>Holds information about the return value.</para>
    /// <para>Since return values have no names, the <see cref="TSParameter.name"/> defaults to "ReturnValue".</para>
    /// </summary>
    public TSParameter ReturnType { get; } = default;

    /// <summary>
    /// Indicates if the <see cref="ReturnType"/> is a promise or not.
    /// </summary>
    public bool ReturnPromise { get; } = false;


    /// <summary>
    /// When not null, this instance could not be instantiated correctly.
    /// </summary>
    public (DiagnosticDescriptor? descriptor, int position) Error { get; private set; }


    /// <summary>
    /// Creates a TSFunction if the given line represents a exported js-function. 
    /// </summary>
    /// <param name="line">An Entire line in a "d.ts"-file.</param>
    /// <returns>null, if not starting with "export declare function ", otherwise tries to parse and returns a <see cref="TSFunction"/>.</returns>
    public static TSFunction? Parse(string line) {
        if (line.StartsWith("export declare function "))
            return new TSFunction(line, 24); // skip "export declare function "
        else if (line.StartsWith("export function "))
            return new TSFunction(line, 16); // skip "export function "
        else
            return null;
    }

    private TSFunction(string line, int position) {
        // FunctionName
        int openBracket = line.IndexOf('(', position);
        if (openBracket == -1) {
            Error = (DiagnosticErrors.ModuleMissingOpenBracket, position);
            return;
        }

        int openGenericBracket = line.IndexOf('<', position, openBracket - position);
        if (openGenericBracket == -1) {
            Name = line[position..openBracket];
            position = openBracket + 1; // skip "("
        }
        else {
            Name = line[position..openGenericBracket];
            position = openGenericBracket + 1;

            // Generics
            List<string> generics = [];

            bool ignoreWhiteSpace = false;
            int bracketCount = 0;
            for (int i = position; true; i++) {
                if (i == line.Length) {
                    Error = (DiagnosticErrors.ModuleMissingClosingGenericBracket, openGenericBracket);
                    return;
                }

                char c = line[i];
                switch (c) {
                    case ' ':
                        if (!ignoreWhiteSpace) {
                            generics.Add(line[position..i]);
                            ignoreWhiteSpace = true; // skip "extends ..."
                        }
                        break;
                    case ',':
                        if (bracketCount == 0) {
                            generics.Add(line[position..i]);
                            i += 2; // skip ", "
                            position = i;
                            ignoreWhiteSpace = false;
                        }
                        break;
                    case '<':
                        bracketCount++;
                        break;
                    case '>':
                        if (bracketCount > 0) {
                            bracketCount--;
                            break;
                        }
                        else {
                            if (!ignoreWhiteSpace)
                                generics.Add(line[position..i]);
                            i += 2; // skip ">("
                            position = i;
                            goto double_break;
                        }
                }
            }
            double_break:

            Generics = [.. generics];
        }


        // Parameters
        if (line[position] == ')')
            position += 3; // no parameters, skip "): "
        else {
            List<TSParameter> parameterList = [];
            while (true) {
                TSParameter tsParameter = new();

                // parse Name
                int colon = line.IndexOf(':', position);
                if (colon == -1) {
                    Error = (DiagnosticErrors.ModuleMissingColon, position);
                    return;
                }
                tsParameter.ParseName(line, position, colon);
                position = colon + 2; // skip ": "

                // parse Type
                int parameterTypeEnd;
                int bracketCount = 0;
                for (int i = position; true; i++) {
                    if (i == line.Length) {
                        Error = (DiagnosticErrors.ModuleNoParameterEnd, position);
                        return;
                    }

                    char c = line[i];
                    switch (c) {
                        case ',':
                            parameterTypeEnd = i;
                            goto double_break;
                        case '(':
                            bracketCount++;
                            break;
                        case ')':
                            if (bracketCount == 0) {
                                parameterTypeEnd = i;
                                goto double_break;
                            }
                            bracketCount--;
                            break;
                    }
                }
                double_break:

                tsParameter.ParseType(line, position, parameterTypeEnd);

                parameterList.Add(tsParameter);

                position = parameterTypeEnd;
                if (line[position] == ',')
                    position += 2; // skip ", "
                else {
                    position += 3; // no parameters, skip "): "
                    break;
                }
            }

            ParameterList = [.. parameterList];
        }


        // ReturnType/Promise
        int semicolon = line.Length - 1;
        if (line[semicolon] != ';') {
            Error = (DiagnosticErrors.ModuleMissingEndingSemicolon, semicolon);
            return;
        }

        if (line.AsSpan(position).StartsWith("Promise<".AsSpan())) {
            ReturnPromise = true;
            // cut "Promise<..>"
            position += 8;
            semicolon--;
        }
        else
            ReturnPromise = false;

        TSParameter parameter = new() { name = "ReturnValue" };
        parameter.ParseType(line, position, semicolon);
        ReturnType = parameter;
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
