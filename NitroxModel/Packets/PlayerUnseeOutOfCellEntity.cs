using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class PlayerUnseeOutOfCellEntity : Packet
{
    public NitroxId EntityId { get; set; }

    public PlayerUnseeOutOfCellEntity(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
