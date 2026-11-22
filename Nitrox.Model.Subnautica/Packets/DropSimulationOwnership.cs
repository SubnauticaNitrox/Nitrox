using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class DropSimulationOwnership(List<NitroxId> entityIds) : Packet
{
    public List<NitroxId> EntityIds { get; } = entityIds;
}
