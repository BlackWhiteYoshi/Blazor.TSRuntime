namespace TSRuntime.Core.Configs.NamePattern;

public struct PreLoadNamePattern : IEquatable<PreLoadNamePattern>
{
    private const string MODULE = "#module#";

    
    private readonly List<OutputBlock> outputList = new(2); // default "PreLoad#module#" are 2 entries
    public string NamePattern { get; }
    public NameTransform ModuleTransform { get; }
    

    public PreLoadNamePattern(string namePattern, NameTransform moduleTransform)
    {
        NamePattern = namePattern;
        ModuleTransform = moduleTransform;


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

            // read in #module#
            if (str.StartsWith(MODULE.AsSpan())) {
                outputList.Add(Output.Module);
                str = str[MODULE.Length..];
            }
            else
               throw new ArgumentException($"Only argument {MODULE} is allowed");
        }
    }

    public IEnumerable<string> GetNaming(string module)
    {
        string moduleName = ModuleTransform.Transform(module);


        foreach (OutputBlock block in outputList)
            yield return block.output switch
            {
                Output.Module => moduleName,
                Output.String => block.content,
                _ => throw new Exception("not reachable")
            };
    }


    #region IEquatable

    public bool Equals(PreLoadNamePattern other) {
        if (NamePattern != other.NamePattern)
            return false;

        if (ModuleTransform != other.ModuleTransform)
            return false;

        return true;
    }

    public override bool Equals(object obj) {
        if (obj is not PreLoadNamePattern other)
            return false;

        return Equals(other);
    }

    public static bool operator ==(PreLoadNamePattern left, PreLoadNamePattern right) {
        return left.Equals(right);
    }

    public static bool operator !=(PreLoadNamePattern left, PreLoadNamePattern right) {
        return !left.Equals(right);
    }

    public override int GetHashCode() {
        int hash = NamePattern.GetHashCode();

        hash = (hash << 5) - hash + ModuleTransform.GetHashCode();

        return hash;
    }

    #endregion
}
