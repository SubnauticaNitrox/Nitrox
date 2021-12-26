using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class DebugStartMapPacket : Packet
    {
        [Index(0)]
        public virtual IList<NitroxVector3> StartPositions { get; protected set; }

        private DebugStartMapPacket() { }

        public DebugStartMapPacket(IList<NitroxVector3> startPositions)
        {
            StartPositions = startPositions;
        }
    }
}
