namespace TSRuntime.Core.Configs;

public struct FunctionNamePattern : IEquatable<FunctionNamePattern>
{
    private const string FUNCTION = "$function$";
    private const string MODULE = "$module$";
    private const string ACTION = "$action$";

    private enum Output
    {
        Function,
        Module,
        Action,
        String
    }

    private struct OutputBlock
    {
        public Output output;
        public string content;

        public static implicit operator OutputBlock(Output output) => new()
        {
            output = output,
            content = string.Empty
        };

        public static implicit operator OutputBlock(string content) => new()
        {
            output = Output.String,
            content = content
        };
    }


    private readonly List<OutputBlock> outputList = new(5); // default "$function$_$module$_$action$" are 5 entries
    public string NamePattern { get; }
    public NameTransform FunctionTransform { get; }
    public NameTransform ModuleTransform { get; }
    public NameTransform ActionTransform { get; }


    public FunctionNamePattern(string namePattern, NameTransform functionTransform, NameTransform moduleTransform, NameTransform actionTransform)
    {
        NamePattern = namePattern;
        FunctionTransform = functionTransform;
        ModuleTransform = moduleTransform;
        ActionTransform = actionTransform;


        ReadOnlySpan<char> str = namePattern.AsSpan();

        while (str.Length > 0)
        {
            int index = str.IndexOf('$');

            // has no "$"
            if (index == -1)
            {
                outputList.Add(str.ToString());
                return;
            }

            // read in ..$
            if (index > 0)
            {
                outputList.Add(str[..index].ToString());
                str = str[index..];
            }

            // read in $..$
            switch (str)
            {
                case { } when str.StartsWith(FUNCTION.AsSpan()):
                    outputList.Add(Output.Function);
                    str = str[FUNCTION.Length..];
                    break;
                case { } when str.StartsWith(MODULE.AsSpan()):
                    outputList.Add(Output.Module);
                    str = str[MODULE.Length..];
                    break;
                case { } when str.StartsWith(ACTION.AsSpan()):
                    outputList.Add(Output.Action);
                    str = str[ACTION.Length..];
                    break;
                default:
                    throw new ArgumentException($"Only arguments {FUNCTION}, {MODULE} or {ACTION} are allowed");
            }
        }
    }

    public IEnumerable<string> GetNaming(string function, string module, string action)
    {
        static string Transform(string name, NameTransform transform) {
            if (name.Length == 0)
                return string.Empty;

            return transform switch {
                NameTransform.None => name,
                NameTransform.UpperCase => name.ToUpper(),
                NameTransform.LowerCase => name.ToLower(),
                NameTransform.FirstUpperCase => $"{char.ToUpperInvariant(name[0])}{name[1..]}",
                NameTransform.FirstLowerCase => $"{char.ToLowerInvariant(name[0])}{name[1..]}",
                _ => throw new ArgumentException("Invalid Enum 'NameTransform'")
            };
        }

        string functionName = Transform(function, FunctionTransform);
        string moduleName = Transform(module, ModuleTransform);
        string actionName = Transform(action, ActionTransform);


        foreach (OutputBlock block in outputList)
            yield return block.output switch
            {
                Output.Function => functionName,
                Output.Module => moduleName,
                Output.Action => actionName,
                Output.String => block.content,
                _ => throw new Exception("not reachable")
            };
    }


    #region IEquatable

    public bool Equals(FunctionNamePattern other) {
        if (NamePattern != other.NamePattern)
            return false;

        if (FunctionTransform != other.FunctionTransform)
            return false;

        if (ModuleTransform != other.ModuleTransform)
            return false;

        if (ActionTransform != other.ActionTransform)
            return false;

        return true;
    }

    public override bool Equals(object obj) {
        if (obj is not FunctionNamePattern other)
            return false;

        return Equals(other);
    }

    public static bool operator ==(FunctionNamePattern left, FunctionNamePattern right) {
        return left.Equals(right);
    }

    public static bool operator !=(FunctionNamePattern left, FunctionNamePattern right) {
        return !left.Equals(right);
    }

    public override int GetHashCode() {
        int hash = NamePattern.GetHashCode();

        hash = (hash << 5) - hash + FunctionTransform.GetHashCode();
        hash = (hash << 5) - hash + ModuleTransform.GetHashCode();
        hash = (hash << 5) - hash + ActionTransform.GetHashCode();

        return hash;
    }

    #endregion
}
