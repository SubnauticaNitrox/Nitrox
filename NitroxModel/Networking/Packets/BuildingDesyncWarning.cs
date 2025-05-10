using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public sealed record BuildingDesyncWarning : Packet
{
    public Dictionary<NitroxId, int> Operations { get; }

    public BuildingDesyncWarning(Dictionary<NitroxId, int> operations)
    {
        Operations = operations;
    }
}
