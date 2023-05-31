namespace TSRuntime.Core.Configs;

public readonly struct MappedType : IEquatable<MappedType>
{
    public readonly string Type { get; init; }
    public readonly GenericType[] GenericTypes { get; init; }

    public MappedType(string type)
    {
        Type = type;
        GenericTypes = Array.Empty<GenericType>();
    }

    public MappedType(string type, string genericType)
    {
        Type = type;
        GenericTypes = new GenericType[1] { new(genericType) };
    }

    public MappedType(string type, GenericType genericType) {
        Type = type;
        GenericTypes = new GenericType[1] { genericType };
    }

    public MappedType(string type, GenericType[] genericTypes)
    {
        Type = type;
        GenericTypes = genericTypes;
    }


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
