using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Nitrox.Model.Core;

/// <summary>
///     Globally unique ID of the networked entity. Is 0 for server. Starts from 1 if player.
/// </summary>
[DebuggerDisplay($"{{{nameof(Id)}}}")]
[DataContract]
[Serializable]
public readonly record struct PeerId : IComparable<PeerId>
{
    public const uint SERVER_ID = 0;

    [DataMember(Order = 1)]
    public readonly uint Id;

    public bool IsServer => Id == SERVER_ID;

    public PeerId(uint id)
    {
        Id = id;
    }

    public static implicit operator uint(PeerId id) => id.Id;

    public static implicit operator PeerId(uint id) => new(id);

    public int CompareTo(PeerId other) => Id.CompareTo(other.Id);
}
