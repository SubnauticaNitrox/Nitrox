using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
///     Health of a creature, sent by its simulating player whenever it changes so that the next simulating player continues from it.
///     Never 0: deaths are replicated by RemoveCreatureCorpse or EntityDestroyed.
/// </summary>
[Serializable]
public class CreatureHealthChanged : Packet
{
    public NitroxId CreatureId { get; }

    public float Health { get; }

    public CreatureHealthChanged(NitroxId creatureId, float health)
    {
        CreatureId = creatureId;
        Health = health;
    }
}
