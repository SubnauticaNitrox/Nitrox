using System;

namespace Nitrox.Model.Core;

/// <summary>
///     Globally unique ID of the networked entity. Is 0 for server. Starts from 1 if player.
/// </summary>
public readonly record struct PeerId : IComparable<PeerId>
{
    public const uint SERVER_ID = 0;

    private readonly uint id;

    public bool IsServer => id == SERVER_ID;

    private PeerId(uint id)
    {
        this.id = id;
    }

    public static implicit operator uint(PeerId id) => id.id;

    public static implicit operator PeerId(uint id) => new(id);
    public int CompareTo(PeerId other) => id.CompareTo(other.id);
}
