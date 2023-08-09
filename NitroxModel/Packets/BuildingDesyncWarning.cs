using System;
using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxModel.Packets;

[Serializable]
public class BuildingDesyncWarning : Packet
{
    public Dictionary<NitroxId, int> Operations { get; }

    public BuildingDesyncWarning(Dictionary<NitroxId, int> operations)
    {
        Operations = operations;
    }
}
