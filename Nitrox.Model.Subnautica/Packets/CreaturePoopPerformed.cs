using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class CreaturePoopPerformed : Packet
{
    public NitroxId CreatureId { get; }

    public CreaturePoopPerformed(NitroxId creatureId)
    {
        CreatureId = creatureId;
    }
}
