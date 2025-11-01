using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class BuildingDesyncWarning : Packet
{
    public Dictionary<NitroxId, int> Operations { get; }

    public BuildingDesyncWarning(Dictionary<NitroxId, int> operations)
    {
        Operations = operations;
    }
}
