using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DebugStartMapPacket : Packet
    {
        public IList<NitroxVector3> StartPositions { get; }

        public DebugStartMapPacket(IList<NitroxVector3> startPositions)
        {
            StartPositions = startPositions;
        }
    }
}
