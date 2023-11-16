namespace TSRuntime.Core.Configs;

public readonly struct MappedType(string type, GenericType[] genericTypes) : IEquatable<MappedType>
{
    public readonly string Type { get; init; } = type;
    public readonly GenericType[] GenericTypes { get; init; } = genericTypes;

    public MappedType(string type) : this(type, Array.Empty<GenericType>()) { }

    public MappedType(string type, string genericType) : this(type, [new GenericType(genericType)]) { }

    public MappedType(string type, GenericType genericType) : this(type, [genericType]) { }


    #region IEquatable

    public readonly bool Equals(MappedType other) {
        if (Type != other.Type)
            return false;

        if (GenericTypes.Length != other.GenericTypes.Length)
            return false;
        for (int i = 0; i < GenericTypes.Length; i++)
            if (GenericTypes[i] != other.GenericTypes[i])
                return false;

        return true;
    }

    public readonly override bool Equals(object obj) {
        if (obj is not MappedType other)
            return false;

        return Equals(other);
    }

    public static bool operator ==(MappedType left, MappedType right) {
        return left.Equals(right);
    }

    public static bool operator !=(MappedType left, MappedType right) {
        return !left.Equals(right);
    }

    public readonly override int GetHashCode() {
        int hash = Type.GetHashCode();

        foreach (GenericType genericType in GenericTypes)
            hash = Combine(hash, genericType.GetHashCode());

        return hash;


        static int Combine(int h1, int h2) {
            uint r = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)r + h1 ^ h2;
        }
    }

    #endregion
}
