using System;

namespace TSRuntime.Core.Parsing;

/// <summary>
/// Represents a js-function inside a <see cref="TSModule"/>.
/// </summary>
public sealed class TSFunction {
    /// <summary>
    /// The name of the js-function. 
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of the parameters of this js-function.
    /// </summary>
    public List<TSParameter> ParameterList { get; set; } = new();

    /// <summary>
    /// <para>Holds information about the return value.</para>
    /// <para>Since return values have no names, the <see cref="TSParameter.Name"/> defaults to "ReturnValue".</para>
    /// </summary>
    public TSParameter ReturnType { get; set; } = new() { Name = "ReturnValue" };
    
    /// <summary>
    /// Indicates if the <see cref="ReturnType"/> is a promise or not.
    /// </summary>
    public bool ReturnPromise { get; set; }


    /// <summary>
    /// Creates a TSFunction if the given line represents a exported js-function. 
    /// </summary>
    /// <param name="line">An Entire line in a "d.ts"-file.</param>
    /// <returns>null, if not starting with "export declare function ", otherwise tries to parse and returns a <see cref="TSFunction"/>.</returns>
    /// <exception cref="Exception">is thrown when a parsing error occurs.</exception>
    public static TSFunction? Parse(ReadOnlySpan<char> line) {
        if (line.StartsWith("export declare function ".AsSpan()))
            line = line[24..]; // skip "export declare function "
        else if (line.StartsWith("export function ".AsSpan()))
            line = line[16..]; // skip "export function "
        else
            return null;

        TSFunction tsFunction = new();

        // FunctionName
        int openBracket = IndexOf(line, '(');
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
                int colon = IndexOf(line, ':');
                tsParameter.ParseName(line[..colon]);
                line = line[(colon + 2)..]; // skip ": "

                // parse Type
                int parameterTypeEnd = IndexOfParameterEnd(line);
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
        int semicolon = IndexOf(line, ';');
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



        static int IndexOf(ReadOnlySpan<char> str, char c) {
            int pos = str.IndexOf(c);
            if (pos != -1)
                return pos;
            else
                throw new Exception($"invalid d.ts file: '{c}' expected");
        }

        static int IndexOfParameterEnd(ReadOnlySpan<char> str) {
            int bracketCount = 0;
            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                switch (c) {
                    case ',':
                        return i;
                    case '(':
                        bracketCount++;
                        break;
                    case ')':
                        if (bracketCount == 0)
                            return i;
                        bracketCount--;
                        break;
                }
            }

            throw new Exception($"invalid d.ts file: no end of parameter found, expected ',' or ')'");
        }
    }
}
