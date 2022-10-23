using System;

namespace NitroxModel.DataStructures;

/// <summary>
///     Serializable version of <see cref="Version" /> with only major and minor properties.
/// </summary>
public readonly struct NitroxVersion : IComparable<NitroxVersion>
{
    public int Major { get; init; }

    public int Minor { get; init; }

    public NitroxVersion(int major, int minor)
    {
        if (major < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(major));
        }
        if (minor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minor));
        }

        Major = major;
        Minor = minor;
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
