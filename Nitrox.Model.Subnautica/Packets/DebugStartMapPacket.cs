using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class DebugStartMapPacket : Packet
    {
        public List<NitroxVector3> StartPositions { get; }

        public DebugStartMapPacket(List<NitroxVector3> startPositions)
        {
            StartPositions = startPositions;
        }
    }
}
