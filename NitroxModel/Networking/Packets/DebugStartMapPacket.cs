using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record DebugStartMapPacket : Packet
    {
        public List<NitroxVector3> StartPositions { get; }

        public DebugStartMapPacket(List<NitroxVector3> startPositions)
        {
            StartPositions = startPositions;
        }
    }
}
