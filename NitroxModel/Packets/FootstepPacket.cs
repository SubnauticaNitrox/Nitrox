using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxModel.Packets
{
    public class FootstepPacket : Packet
    {
        public readonly ushort playerID;
        public readonly byte assetIndex;
        public FootstepPacket(ushort playerID, byte assetIndex)
        {
            Log.Info("Creating footstep packet");
            this.playerID = playerID;
            this.assetIndex = assetIndex;
        }
    }
}
