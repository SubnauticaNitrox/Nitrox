using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PlayerUnseeOutOfCellEntity : Packet
{
    public NitroxId EntityId { get; set; }

    public PlayerUnseeOutOfCellEntity(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
