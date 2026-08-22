using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class DiveReelNodesInitialSync : Packet
{
    public Dictionary<ushort, List<NitroxVector3>> PlayerNodes { get; }

    public DiveReelNodesInitialSync(Dictionary<ushort, List<NitroxVector3>> playerNodes)
    {
        PlayerNodes = playerNodes;
    }

    public override string ToString()
    {
        return $"[{nameof(DiveReelNodesInitialSync)} PlayerNodes: {PlayerNodes.Count} players]";
    }
}
