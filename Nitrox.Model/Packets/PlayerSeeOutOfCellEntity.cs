using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class PlayerSeeOutOfCellEntity : Packet
{
    public NitroxId EntityId { get; set; }

    public PlayerSeeOutOfCellEntity(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
