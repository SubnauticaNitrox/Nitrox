using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class CreaturePoopPerformed : Packet
{
    public NitroxId CreatureId { get; }

    public CreaturePoopPerformed(NitroxId creatureId)
    {
        CreatureId = creatureId;
    }
}
