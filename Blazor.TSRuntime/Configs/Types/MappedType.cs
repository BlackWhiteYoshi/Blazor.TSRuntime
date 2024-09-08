namespace TSRuntime.Configs;

public readonly struct MappedType(string type, GenericType[] genericTypes) : IEquatable<MappedType> {
    public readonly string Type { get; init; } = type;
    public readonly GenericType[] GenericTypes { get; init; } = genericTypes;

    public MappedType(string type) : this(type, []) { }

    public MappedType(string type, string genericType) : this(type, [new GenericType(genericType)]) { }

    public MappedType(string type, GenericType genericType) : this(type, [genericType]) { }


    #region IEquatable

    public static bool operator ==(MappedType left, MappedType right) => left.Equals(right);

    public static bool operator !=(MappedType left, MappedType right) => !left.Equals(right);

    public override bool Equals(object obj)
        => obj switch {
            MappedType other => Equals(other),
            _ => false
        };

    public bool Equals(MappedType other) {
        if (Type != other.Type)
            return false;

        if (!GenericTypes.SequenceEqual(other.GenericTypes))
            return false;

        return true;
    }

    public override int GetHashCode() {
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
