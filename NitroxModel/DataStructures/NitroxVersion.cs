using System;

namespace NitroxModel.DataStructures;

/// <summary>
///     Serializable version of <see cref="Version" /> with only major and minor properties.
/// </summary>
public readonly struct NitroxVersion : IComparable<NitroxVersion>
{
    public ushort Major { get; init; }

    public ushort Minor { get; init; }


    public NitroxVersion(ushort major, ushort minor)
    {
        Major = major;
        Minor = minor;
    }

    public NitroxVersion(int major, int minor) : this((ushort)major, (ushort)minor)
    {
        if (major is < 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(major));
        }
        if (minor is < 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(minor));
        }
    }

    public int CompareTo(NitroxVersion other)
    {
        if (Major != other.Major)
        {
            return Major > other.Major ? 1 : -1;
        }
        if (Minor != other.Minor)
        {
            return Minor > other.Minor ? 1 : -1;
        }

        return 0;
    }

    public override string ToString() => $"{Major}.{Minor}";
}
