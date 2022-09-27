using System.Text;

namespace TSRuntime.Generation;

internal static class BuilderExtension {
    internal static void Append(this StringBuilder builder, ReadOnlySpan<char> str) {
        foreach (char c in str)
            builder.Append(c);
    }
}

public sealed class Parser {
    private readonly StringBuilder builder = new(65536);

    /// <summary>
    /// Creates a string out of the <see cref="StringBuilder"/>.
    /// </summary>
    /// <returns></returns>
    public string GetContent() => builder.ToString();


    /// <summary>
    /// Parses the string and adds the converted string to the <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="str"></param>
    /// <exception cref="Exception"></exception>
    public void Parse(ReadOnlySpan<char> str) {
        int index;
        int indentation = 2;
        while (str.Length > 0) {
            index = str.IndexOf('`');
            if (index == -1) {
                WriteString(str, indentation);
                break;
            }

            if (index > 0)
                WriteString(str.Slice(0, index), indentation);

            // double tick
            if (str[index + 1] == '`') {
                str = str.Slice(index + 2);

                index = str.IndexOf('`');
                if (index == -1)
                    throw new Exception($"missing ending double `+, `- or ``, at {str.Slice(0, index).ToString()}");

                switch (str[index + 1]) {
                    case '+':
                        WriteCode(str.Slice(0, index), indentation);
                        indentation++;
                        break;
                    case '-':
                        indentation--;
                        WriteCode(str.Slice(0, index), indentation);
                        break;
                    case '`':
                        WriteCode(str.Slice(0, index), indentation);
                        break;
                    default:
                        throw new Exception($"`+, `- or `` expected, only single ` found, at {str.Slice(0, index).ToString()}");
                }

                str = str.Slice(index + 2);
                if (str.Length >= Environment.NewLine.Length)
                    if (str.Slice(0, Environment.NewLine.Length).CompareTo(Environment.NewLine.AsSpan(), StringComparison.Ordinal) == 0)
                        str = str.Slice(Environment.NewLine.Length);
            }
            // single tick
            else {
                str = str.Slice(index + 1);

                index = str.IndexOf('`');
                if (index == -1)
                    throw new Exception($"missing single `, at {str.Slice(0, index).ToString()}");

                WriteVar(str.Slice(0, index), indentation);

                str = str.Slice(index + 1);
            }
        }
    }


    private void WriteString(ReadOnlySpan<char> str, int indentation) {
        Indent(indentation);
        builder.Append("yield return \"\"\"");
        builder.AppendLine();

        IndentWriting(str, indentation + 1);
        builder.AppendLine();

        Indent(indentation + 1);
        builder.Append("\"\"\";");
        builder.AppendLine();
    }

    private void WriteVar(ReadOnlySpan<char> var, int indentation) {
        Indent(indentation);

        builder.Append("yield return ");
        builder.Append(var);
        builder.Append(';');

        builder.AppendLine();
    }

    private void WriteCode(ReadOnlySpan<char> code, int indentation) {
        IndentWriting(code.Trim(), indentation);
        builder.AppendLine();
    }


    private void IndentWriting(ReadOnlySpan<char> lines, int indentation) {
        while (true) {
            Indent(indentation);

            int nextPos = lines.IndexOf(Environment.NewLine.AsSpan());
            if (nextPos == -1) {
                builder.Append(lines);
                break;
            }
            builder.Append(lines.Slice(0, nextPos));
            lines = lines.Slice(nextPos + Environment.NewLine.Length);

            builder.AppendLine();
        }
    }

    private void Indent(int indentation) => builder.Append(' ', 4 * indentation);
}
