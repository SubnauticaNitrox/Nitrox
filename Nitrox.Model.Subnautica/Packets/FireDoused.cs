using System;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
///     Triggered when a fire has been doused. Fire growth is a static thing, so we only need to track dousing
/// </summary>
[Serializable]
public sealed class FireDoused : Packet
{
    /// <param name="id">The Fire id</param>
    /// <param name="health">The new health of the fire. If less than zero, fire is extinguished.</param>
    /// <param name="douseAmount">The decrease in health from the old health.</param>
    public FireDoused(NitroxId id, float health, float douseAmount)
    {
        Id = id;
        Health = health;
        DouseAmount = douseAmount;
    }

    public NitroxId Id { get; }
    public float Health { get; }
    public float DouseAmount { get; }
    [IgnoredMember]
    public bool IsExtinguished => Health <= 0;
}
