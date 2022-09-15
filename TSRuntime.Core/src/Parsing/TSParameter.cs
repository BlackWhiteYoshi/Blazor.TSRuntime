namespace TSRuntime.Core.Parsing;

public sealed class TSParameter {
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool TypeNullable { get; set; }
    public bool Array { get; set; }
    public bool ArrayNullable { get; set; }


    public void ParseType(ReadOnlySpan<char> subStr) {
        /** e.g.
         * 
         * number
         * number | null
         * number[]
         * Array<number>
         * (number | null)[]
         * (number | null)[] | null
         * 
         **/

        bool nullable;
        Range newRange;

        (nullable, newRange) = ParseNullable(subStr);
        if (nullable)
            subStr = subStr[newRange];

        if (subStr.EndsWith("[]")) {
            Array = true;
            ArrayNullable = nullable;
            subStr = subStr[..2];   // cut "..[]"
            if (subStr[0] == '(') {
                subStr = subStr[1..^1]; // cut "(..)"
                (nullable, newRange) = ParseNullable(subStr);
                if (nullable)
                    subStr = subStr[newRange];
            }
            else
                nullable = false;
        }
        else if (subStr.StartsWith("Array<")) {
            Array = true;
            ArrayNullable = nullable;
            subStr = subStr[6..^1];   // cut "Array<..>"
            (nullable, newRange) = ParseNullable(subStr);
            if (nullable)
                subStr = subStr[newRange];
        }

        TypeNullable = nullable;
        Type = new string(subStr);
        return;


        static (bool nullable, Range newRange) ParseNullable(ReadOnlySpan<char> subStr) {
            if (subStr.StartsWith("null |"))
                return (true, new Range(Index.FromStart(7), 0));    // cut "null | .."
            
            if (subStr.StartsWith("undefined |"))
                return (true, new Range(Index.FromStart(12), 0));   // cut "undefined | .."

            if (subStr.EndsWith("| null"))
                return (true, new Range(0, Index.FromEnd(7)));    // cut "..| null"
            
            if (subStr.EndsWith("| undefined"))
                return (true, new Range(0, Index.FromEnd(12)));   // cut "..| undefined"

            return (false, default);
        }
    }
}
