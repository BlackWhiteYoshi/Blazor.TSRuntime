using System.Text;

namespace TSRuntime.Generation;

public static class Parser {
    public static string Parse(string str) => new Core().Parse(str.AsSpan());

    
    private static void Append(this StringBuilder builder, ReadOnlySpan<char> str) {
        foreach (char c in str)
            builder.Append(c);
    }

    private readonly struct Core() {
        private readonly StringBuilder builder = new(65536);
    

        /// <summary>
        /// Converts the string to valid C# code.
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="Exception"></exception>
        public readonly string Parse(ReadOnlySpan<char> str) {
            int index;
            int indentation = 2;
            while (str.Length > 0) {
                index = str.IndexOf('`');
                if (index == -1) {
                    WriteString(str, indentation);
                    break;
                }

                if (index > 0)
                    WriteString(str[..index], indentation);

                // double tick
                if (str[index + 1] == '`') {
                    str = str[(index + 2)..];

                    index = str.IndexOf('`');
                    if (index == -1)
                        throw new Exception($"missing ending double `+, `- or ``, at {str[..index].ToString()}");

                    switch (str[index + 1]) {
                        case '+':
                            WriteCode(str[..index], indentation);
                            indentation++;
                            break;
                        case '-':
                            indentation--;
                            WriteCode(str[..index], indentation);
                            break;
                        case '`':
                            WriteCode(str[..index], indentation);
                            break;
                        default:
                            throw new Exception($"`+, `- or `` expected, only single ` found, at {str[..index].ToString()}");
                    }

                    str = str[(index + 2)..];
                    if (str is ['\n', .. ReadOnlySpan<char> remaining])
                        str = remaining;
                }
                // single tick
                else {
                    str = str[(index + 1)..];

                    index = str.IndexOf('`');
                    if (index == -1)
                        throw new Exception($"missing single `, at {str[..index].ToString()}");

                    WriteVar(str[..index], indentation);

                    str = str[(index + 1)..];
                }
            }

            if (builder.Length > 0)
                builder.Length--;
            return builder.ToString();
        }


        private readonly void WriteString(ReadOnlySpan<char> str, int indentation) {
            Indent(indentation);
            builder.Append("yield return \"\"\"\n");

            IndentWriting(str, indentation + 1);
            builder.Append('\n');

            Indent(indentation + 1);
            builder.Append("\"\"\";\n");
        }

        private readonly void WriteVar(ReadOnlySpan<char> var, int indentation) {
            Indent(indentation);

            builder.Append("yield return ");
            builder.Append(var);
            builder.Append(';');
            builder.Append('\n');
        }

        private readonly void WriteCode(ReadOnlySpan<char> code, int indentation) {
            IndentWriting(code.Trim(), indentation);
            builder.Append('\n');
        }


        private readonly void IndentWriting(ReadOnlySpan<char> lines, int indentation) {
            while (true) {
                Indent(indentation);

                int nextPos = lines.IndexOf('\n');
                if (nextPos == -1) {
                    builder.Append(lines);
                    break;
                }
                builder.Append(lines[..nextPos]);
                lines = lines[(nextPos + 1)..];

                builder.Append('\n');
            }
        }

        private readonly void Indent(int indentation) => builder.Append(' ', 4 * indentation);
    }
}
