using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class PlayerUnseeOutOfCellEntity : Packet
{
    public NitroxId EntityId { get; set; }

    public PlayerUnseeOutOfCellEntity(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
