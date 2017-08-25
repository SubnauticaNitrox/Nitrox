using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionBegin : PlayerActionPacket
    { 
        public String Guid { get; private set; }

        public DeconstructionBegin(String playerId, Vector3 itemPosition, String guid) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[DeconstructionBegin( - playerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
