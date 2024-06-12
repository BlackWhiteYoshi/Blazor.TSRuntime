using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;

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
    /// List of the generic parameters of this js function
    /// </summary>
    public string[] Generics { get; private set; } = [];

    /// <summary>
    /// List of the parameters of this js-function.
    /// </summary>
    public TSParameter[] ParameterList { get; private set; } = [];

    /// <summary>
    /// <para>Holds information about the return value.</para>
    /// <para>Since return values have no names, the <see cref="TSParameter.name"/> defaults to "ReturnValue".</para>
    /// </summary>
    public ref TSParameter ReturnType => ref _returnType;
    private TSParameter _returnType = new() { name = "ReturnValue", type = "void" };

    /// <summary>
    /// Indicates if the <see cref="ReturnType"/> is a promise or not.
    /// </summary>
    public bool ReturnPromise { get; private set; } = false;

    /// <summary>
    /// Description above the js-function.
    /// </summary>
    public string Summary => _summary;
    private string _summary = string.Empty;

    /// <summary>
    /// Description above the js-function after @remarks tag.
    /// </summary>
    public string Remarks => _remarks;
    private string _remarks = string.Empty;


    /// <summary>
    /// When not null, this instance could not be instantiated correctly.
    /// </summary>
    public (DiagnosticDescriptor? descriptor, int position) Error { get; private set; }


    private TSFunction() { }


    /// <summary>
    /// Creates a TSFunction if the given line represents a exported ts-function. 
    /// </summary>
    /// <param name="line">An Entire line in a "d.ts"-file.</param>
    /// <returns>null, if not starting with "export function " or "export declare function ", otherwise tries to parse and returns a <see cref="TSFunction"/>.</returns>
    public static TSFunction? ParseTSFunction(ReadOnlySpan<char> line) {
        int position;
        switch (line) {
            case ['e', 'x', 'p', 'o', 'r', 't', ' ', 'f', 'u', 'n', 'c', 't', 'i', 'o', 'n', ' ', ..]:
                position = 16; // skip "export function "
                break;
            case ['e', 'x', 'p', 'o', 'r', 't', ' ', 'd', 'e', 'c', 'l', 'a', 'r', 'e', ' ', 'f', 'u', 'n', 'c', 't', 'i', 'o', 'n', ' ', ..]:
                position = 24; // skip "export declare function "
                break;
            default:
                return null;
        }


        TSFunction result = new();


        // FunctionName
        int openBracket = line[position..].IndexOf('(');
        if (openBracket == -1) {
            result.Error = (DiagnosticErrors.FileMissingOpenBracket, position);
            return result;
        }
        openBracket += position;

        int openGenericBracket = line[position..openBracket].IndexOf('<');
        if (openGenericBracket == -1) {
            result.Name = line[position..openBracket].ToString();
            position = openBracket + 1; // skip "("
        }
        else {
            openGenericBracket += position;
            result.Name = line[position..openGenericBracket].ToString();
            position = openGenericBracket + 1;

            // Generics
            List<string> generics = [];

            bool ignoreWhiteSpace = false;
            int bracketCount = 0;
            for (int i = position; true; i++) {
                if (i == line.Length) {
                    result.Error = (DiagnosticErrors.FileMissingClosingGenericBracket, openGenericBracket);
                    return result;
                }

                char c = line[i];
                switch (c) {
                    case ' ':
                        if (!ignoreWhiteSpace) {
                            generics.Add(line[position..i].ToString());
                            ignoreWhiteSpace = true; // skip "extends ..."
                        }
                        break;
                    case ',':
                        if (bracketCount == 0) {
                            generics.Add(line[position..i].ToString());
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
                                generics.Add(line[position..i].ToString());
                            i += 2; // skip ">("
                            position = i;
                            goto double_break;
                        }
                }
            }
            double_break:

            result.Generics = [.. generics];
        }


        // Parameters
        if (line[position] == ')')
            position += 3; // no parameters, skip "): "
        else {
            List<TSParameter> parameterList = [];

            while (true) {
                TSParameter tsParameter = new();

                // parse Name
                int colon = line[position..].IndexOf(':');
                if (colon == -1) {
                    result.Error = (DiagnosticErrors.FileMissingColon, position);
                    return result;
                }
                colon += position;
                tsParameter.ParseTSName(line[position..colon]);
                position = colon + 2; // skip ": "

                // parse Type
                int parameterTypeEnd;
                int bracketCount = 0;
                for (int i = position; true; i++) {
                    if (i == line.Length) {
                        result.Error = (DiagnosticErrors.FileNoParameterEnd, position);
                        return result;
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

                tsParameter.ParseType(line[position..parameterTypeEnd]);

                parameterList.Add(tsParameter);

                position = parameterTypeEnd;
                if (line[position] == ',')
                    position += 2; // skip ", "
                else {
                    position += 3; // no parameters, skip "): "
                    break;
                }
            }

            result.ParameterList = [.. parameterList];
        }


        // ReturnType/Promise
        int semicolon = line.Length - 1;
        if (line[semicolon] != ';') {
            result.Error = (DiagnosticErrors.FileMissingEndingSemicolon, semicolon);
            return result;
        }

        if (line[position..] is ['P', 'r', 'o', 'm', 'i', 's', 'e', '<', ..]) {
            result.ReturnPromise = true;
            // cut "Promise<..>"
            position += 8;
            semicolon--;
        }
        else
            result.ReturnPromise = false;

        result._returnType.ParseType(line[position..semicolon]);


        return result;
    }

    /// <summary>
    /// Creates a TSFunction if the given line represents a exported js-function. 
    /// </summary>
    /// <param name="line">An Entire line in a "js"-file.</param>
    /// <returns>null, if not starting with "export function", otherwise tries to parse and returns a <see cref="TSFunction"/>.</returns>
    public static TSFunction? ParseJSFunction(ReadOnlySpan<char> line) {
        if (line is not ['e', 'x', 'p', 'o', 'r', 't', ' ', ..])
            return null;

        ReadOnlySpan<char> lineView = line[7..];
        int position = 7; // skip "export "
        TrimWhiteSpace(ref lineView, ref position);

        if (lineView is not ['f', 'u', 'n', 'c', 't', 'i', 'o', 'n', ' ', ..])
            return null;

        lineView = lineView[9..];
        position += 9; // skip "function "
        TrimWhiteSpace(ref lineView, ref position);


        TSFunction result = new();

        // FunctionName
        int openBracket = lineView.IndexOf('(');
        if (openBracket == -1) {
            result.Error = (DiagnosticErrors.FileMissingOpenBracket, position);
            return result;
        }
        result.Name = lineView[..openBracket].TrimEnd().ToString();

        // skip "("
        if (openBracket + 1 == lineView.Length) {
            result.Error = (DiagnosticErrors.FileNoParameterEnd, position + lineView.Length);
            return result;
        }
        lineView = lineView[(openBracket + 1)..];
        position += openBracket + 1;
        TrimWhiteSpace(ref lineView, ref position);

        // no Parameters
        if (lineView[0] == ')')
            return result;

        // Paramters
        List<TSParameter> parameterList = [];

        while (true) {
            TSParameter tsParameter = new() {
                type = "object",
                typeNullable = true
            };

            int parameterEnd = lineView.IndexOfAny([',', ')']);
            if (parameterEnd == -1) {
                result.Error = (DiagnosticErrors.FileNoParameterEnd, position);
                return result;
            }
            
            tsParameter.ParseJSName(lineView[..parameterEnd].TrimEnd());
            parameterList.Add(tsParameter);

            if (lineView[parameterEnd] == ')')
                break;

            if (parameterEnd + 1 == lineView.Length) {
                result.Error = (DiagnosticErrors.FileNoParameterEnd, position + lineView.Length);
                return result;
            }
            lineView = lineView[(parameterEnd + 1)..];
            position += parameterEnd + 1;
            TrimWhiteSpace(ref lineView, ref position);
        }

        result.ParameterList = [.. parameterList];

        return result;



        static void TrimWhiteSpace(ref ReadOnlySpan<char> lineView, ref int position) {
            int length = lineView.Length;
            lineView = lineView.TrimStart();
            position += length - lineView.Length;
        }
    }


    /// <summary>
    /// Parsing the TSDoc to fill <see cref="Summary"/>, <see cref="Remarks"/> and <see cref="TSParameter.summary"/>.
    /// </summary>
    /// <param name="fileContent"></param>
    /// <param name="position">start of the function definition</param>
    public void ParseSummary(string fileContent, int position, bool isJSDoc) {
        if (position < 6) // at least "/***/\n"
            return;
        
        // find "*/" in the line above
        for (position -= 2; !(fileContent[position - 1] is '*' && fileContent[position] is '/'); position--)
            if (fileContent[position] is '\n' || position < 1)
                return;

        // find "/**"
        int summaryEnd = position - 1;
        int summaryStart = fileContent.LastIndexOf("/*", summaryEnd);
        if (summaryStart == -1)
            return;
        if (fileContent[summaryStart + 2] != '*' || summaryStart + 2 == summaryEnd)
            return;
        summaryStart += 3;


        ReadOnlySpan<char> JSsummary = fileContent.AsSpan(summaryStart, summaryEnd - summaryStart);
        if (JSsummary.Length == 0)
            return;

        ref string CSsummary = ref _summary;
        bool unkownTag = false;

        StringBuilder builder = new();
        int lineStart = 0;
        while (lineStart < JSsummary.Length) {
            // find end of line: '\n' or "*/"
            int lineEnd = JSsummary[lineStart..].IndexOf('\n');
            if (lineEnd != -1)
                lineEnd += lineStart;
            else
                lineEnd = JSsummary.Length;

            ReadOnlySpan<char> line = JSsummary[lineStart..lineEnd];

            // trim " * [...]  "
            line = line.Trim();
            if (line.Length > 0) {
                if (line[0] == '*')
                    line = line[1..].TrimStart();

                int tagIndex = line.IndexOf('@');
                while (tagIndex != -1) {
                    ReadOnlySpan<char> beforeTag = line[..tagIndex].TrimEnd();
                    foreach (char c in beforeTag)
                        builder.Append(c);

                    if (!unkownTag)
                        CSsummary = ToSummary(builder);
                    builder.Clear();

                    line = line[tagIndex..];
                    switch (line) {
                        case ['@', 'p', 'a', 'r', 'a', 'm', ' ', ..]: {
                            line = line[7..].TrimStart(); // skip "@param "

                            ReadOnlySpan<char> typeSpan = isJSDoc ? FindTypeSpan(ref line) : [];
                            for (int i = 0; i < ParameterList.Length; i++)
                                if (line.StartsWith(ParameterList[i].name.AsSpan())) {
                                    CSsummary = ref ParameterList[i].summary;
                                    unkownTag = false;
                                    if (typeSpan.Length > 0)
                                        ParameterList[i].ParseType(typeSpan);

                                    if (line.Length == ParameterList[i].name.Length)
                                        line = [];
                                    else if (char.IsWhiteSpace(line[ParameterList[i].name.Length])) {
                                        line = line[(ParameterList[i].name.Length + 1)..]; // skip "[name] "
                                        if (line.Length >= 2 && line[..2] is ['-', ' ', ..])
                                            line = line[2..]; // skip "- "
                                    }
                                    goto double_break;
                                }

                            unkownTag = true;
                            int paramterEnd = line.IndexOf(' ');
                            if (paramterEnd != -1)
                                line = line[paramterEnd..].TrimStart();
                            else
                                line = []; // skip line, has nothing relevant
                            break;
                        }
                        case ['@', 'r', 'e', 't', 'u', 'r', 'n', 's', ..]: {
                            if (line.Length == 8) {
                                CSsummary = ref _returnType.summary;
                                unkownTag = false;
                                line = []; // line ends after tag
                                break;
                            }

                            if (!char.IsWhiteSpace(line[8])) {
                                line = line[8..];
                                goto default;
                            }

                            CSsummary = ref _returnType.summary;
                            unkownTag = false;
                            line = line[9..].TrimStart(); // skip "@returns "

                            ReadOnlySpan<char> typeSpan = isJSDoc ? FindTypeSpan(ref line) : [];
                            if (typeSpan.Length > 0)
                                _returnType.ParseType(typeSpan);
                            break;
                        }
                        case ['@', 'r', 'e', 'm', 'a', 'r', 'k', 's', ..]: {
                            if (line.Length == 8) {
                                CSsummary = ref _remarks;
                                unkownTag = false;
                                line = []; // line ends after tag
                                break;
                            }

                            if (!char.IsWhiteSpace(line[8])) {
                                line = line[8..];
                                goto default;
                            }

                            CSsummary = ref _remarks;
                            unkownTag = false;
                            line = line[9..].TrimStart(); // skip "@remarks "
                            break;
                        }
                        default: {
                            unkownTag = true;
                            for (int i = 1; i < line.Length; i++)
                                if (char.IsWhiteSpace(line[i])) {
                                    line = line[i..];
                                    goto double_break;
                                }

                            line = []; // line ends after tag
                            break;
                        }
                    }
                    double_break:
                    tagIndex = line.IndexOf('@');
                }

                line = line.TrimEnd();
                foreach (char c in line)
                    builder.Append(c);
            }

            builder.Append("<br/>");
            lineStart = lineEnd + 1;
        }

        if (!unkownTag)
            CSsummary = ToSummary(builder);



        static ReadOnlySpan<char> FindTypeSpan(ref ReadOnlySpan<char> line) {
            if (line is not ['{', ..])
                return [];

            int bracketCount = 0;
            for (int i = 1; i < line.Length; i++)
                switch (line[i]) {
                    case '{':
                        bracketCount++;
                        break;
                    case '}':
                        if (bracketCount > 0) {
                            bracketCount--;
                            break;
                        }

                        ReadOnlySpan<char> typeSpan = line[1..i].Trim();
                        if (line.Length - 1 == i)
                            line = []; // end of line
                        else
                            line = line[(i + 1)..].TrimStart(); // skip "{...}"
                        return typeSpan;
                }

            // no matching '}'
            line = [];
            return [];
        }
        
        // removes all leading and trainling "<br/>" and then calls ToString
        static string ToSummary(StringBuilder builder) {
            while (builder.Length >= 5 && builder[^5] == '<' && builder[^4] == 'b' && builder[^3] == 'r' && builder[^2] == '/' && builder[^1] == '>')
                builder.Length -= 5;

            int startIndex = 0;
            while (builder.Length >= startIndex + 5 && builder[startIndex] == '<' && builder[startIndex + 1] == 'b' && builder[startIndex + 2] == 'r' && builder[startIndex + 3] == '/' && builder[startIndex + 4] == '>')
                startIndex += 5;

            return builder.ToString(startIndex, builder.Length - startIndex);
        }
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

        if (!Generics.SequenceEqual(other.Generics))
            return false;

        if (!ParameterList.SequenceEqual(other.ParameterList))
            return false;

        if (ReturnType != other.ReturnType)
            return false;

        if (ReturnPromise != other.ReturnPromise)
            return false;

        if (Summary != other.Summary)
            return false;

        if (Remarks != other.Remarks)
            return false;

        return true;
    }

    public override int GetHashCode() {
        int hash = Name.GetHashCode();

        foreach (string generic in Generics)
            hash = Combine(hash, generic.GetHashCode());

        foreach (TSParameter tsParameter in ParameterList)
            hash = Combine(hash, tsParameter.GetHashCode());

        hash = Combine(hash, ReturnType.GetHashCode());
        hash = Combine(hash, ReturnPromise.GetHashCode());

        hash = Combine(hash, Summary.GetHashCode());
        hash = Combine(hash, Remarks.GetHashCode());

        return hash;


        static int Combine(int h1, int h2) {
            uint r = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)r + h1 ^ h2;
        }
    }

    #endregion
}
