using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerUnseeOutOfCellEntity(NitroxId entityId) : Packet
{
    public NitroxId EntityId { get; set; } = entityId;
}
