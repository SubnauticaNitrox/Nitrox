using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : PlayerActionPacket
    { 
        public String Guid { get; private set; }

        public DeconstructionCompleted(String playerId, Vector3 itemPosition, String guid) : base(playerId, itemPosition)
        {
            this.Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionCompleted( - playerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
