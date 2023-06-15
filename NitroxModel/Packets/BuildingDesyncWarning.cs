using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxModel.Packets;

public class BuildingDesyncWarning : Packet
{
    public Dictionary<NitroxId, int> Operations { get; set; }

    public BuildingDesyncWarning(Dictionary<NitroxId, int> operations)
    {
        Operations = operations;
    }
}
