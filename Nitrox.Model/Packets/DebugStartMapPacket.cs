using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Packets
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
