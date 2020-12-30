using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DebugStartMapPacket : Packet
    {
        public List<NitroxVector3> StartPositions { get; set; }


        public DebugStartMapPacket(List<NitroxVector3> startPositions)
        {
            StartPositions = startPositions;
        }
    }
}
