using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerSeeOutOfCellEntity : Packet
{
    public NitroxId EntityId { get; set; }

    public PlayerSeeOutOfCellEntity(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
