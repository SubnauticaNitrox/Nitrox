using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public sealed class BuildingDesyncWarning : Packet
{
    public Dictionary<NitroxId, int> Operations { get; }

    public BuildingDesyncWarning(Dictionary<NitroxId, int> operations)
    {
        Operations = operations;
    }
}
