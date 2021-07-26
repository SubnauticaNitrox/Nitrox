using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures.GameLogic;

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

        public override string ToString()
        {
            StringBuilder stringBuilder = new("[DebugStartMapPacket - StartPositions : {");

            foreach (NitroxVector3 vector in StartPositions)
            {
                stringBuilder.Append(vector);
            }

            stringBuilder.Append("}]");

            return stringBuilder.ToString();
        }
    }
}
