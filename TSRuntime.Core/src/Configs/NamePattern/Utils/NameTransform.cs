namespace TSRuntime.Core.Configs.NamePattern;

public enum NameTransform
{
    None,
    FirstUpperCase,
    FirstLowerCase,
    UpperCase,
    LowerCase
}

internal static class NameTransformExtension {
    internal static string Transform(this NameTransform transform, string name) {
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
}
