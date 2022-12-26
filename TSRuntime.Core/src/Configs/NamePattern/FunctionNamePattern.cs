namespace TSRuntime.Core.Configs.NamePattern;

public struct FunctionNamePattern : IEquatable<FunctionNamePattern>
{
    private const string FUNCTION = "#function#";
    private const string MODULE = "#module#";
    private const string ACTION = "#action#";

    
    private readonly List<OutputBlock> outputList = new(1); // default "#function#" is 1 entry
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
            int index = str.IndexOf('#');

            // has no "#"
            if (index == -1)
            {
                outputList.Add(str.ToString());
                return;
            }

            // read in ..#
            if (index > 0)
            {
                outputList.Add(str[..index].ToString());
                str = str[index..];
            }

            // read in #..#
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
        string functionName = FunctionTransform.Transform(function);
        string moduleName = ModuleTransform.Transform(module);
        string actionName = ActionTransform.Transform(action);


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
