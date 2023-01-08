namespace TSRuntime.Core.Configs.NamePattern;

/// <summary>
/// Transforms a placeholder in the name pattern.
/// </summary>
public enum NameTransform
{
    /// <summary>
    /// No Transform.
    /// </summary>
    None,

    /// <summary>
    /// Changes the first letter to uppercase.
    /// </summary>
    FirstUpperCase,

    /// <summary>
    /// Changes the first letter to lowercase.
    /// </summary>
    FirstLowerCase,

    /// <summary>
    /// Changes all letters to uppercase.
    /// </summary>
    UpperCase,

    /// <summary>
    /// Changes all letters to lowercase.
    /// </summary>
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
