using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class CreaturePoopPerformed : Packet
{
    public NitroxId CreatureId { get; }

    public CreaturePoopPerformed(NitroxId creatureId)
    {
        CreatureId = creatureId;
    }
}
