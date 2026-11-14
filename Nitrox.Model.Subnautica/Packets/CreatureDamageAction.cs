using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
///     A hit on a creature by a player who doesn't simulate it. The server sends it to the creature's simulating player, the only one
///     changing the creature's health, or back to the sender when nobody simulates the creature.
/// </summary>
[Serializable]
public class CreatureDamageAction : Packet
{
    /// <summary>
    ///     How many times a player who couldn't apply the hit (creature not loaded) can send it back to the server so that it reaches
    ///     the current simulating player.
    /// </summary>
    public const byte MAX_HOPS = 2;

    public NitroxId CreatureId { get; }

    /// <summary>
    ///     Damage before DamageSystem.CalculateDamage, which the receiver runs again through LiveMixin.TakeDamage.
    ///     Can be the sum of several hits of the same type dealt within a short time.
    /// </summary>
    public float OriginalDamage { get; }

    public DamageType Type { get; }

    /// <summary>
    ///     Hit position in the creature's local space because the creature isn't at exactly the same place for every player.
    ///     Zero means the creature's position, like when no position is given to LiveMixin.TakeDamage.
    /// </summary>
    public NitroxVector3 LocalPosition { get; }

    /// <summary>
    ///     Id of the object given as dealer to LiveMixin.TakeDamage (the player, a vehicle, a thrown object), if it has one.
    /// </summary>
    public Optional<NitroxId> DealerId { get; }

    /// <summary>
    ///     Times this hit was sent back to the server by a player who couldn't apply it, see <see cref="MAX_HOPS" />.
    /// </summary>
    public byte Hops { get; }

    public CreatureDamageAction(NitroxId creatureId, float originalDamage, DamageType type, NitroxVector3 localPosition, Optional<NitroxId> dealerId, byte hops)
    {
        CreatureId = creatureId;
        OriginalDamage = originalDamage;
        Type = type;
        LocalPosition = localPosition;
        DealerId = dealerId;
        Hops = hops;
    }
}
