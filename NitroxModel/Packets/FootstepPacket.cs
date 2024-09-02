using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NitroxModel.Packets
{
    [Serializable]
    public class FootstepPacket : Packet
    {
        public ushort playerID { get; }
        public byte assetIndex { get; }
        public FootstepPacket(ushort playerID, byte assetIndex)
        {
            Log.Info("Creating footstep packet");
            this.playerID = playerID;
            this.assetIndex = assetIndex;
        }
    }
}
