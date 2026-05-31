using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class DebugStartMapPacket(IList<NitroxVector3> startPositions) : Packet
{
    public IList<NitroxVector3> StartPositions { get; } = startPositions;
}
