using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerUnseeOutOfCellEntity : Packet
{
    public NitroxId EntityId { get; set; }

    public PlayerUnseeOutOfCellEntity(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
