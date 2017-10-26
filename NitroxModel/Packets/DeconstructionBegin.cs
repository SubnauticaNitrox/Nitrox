using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionBegin : PlayerActionPacket
    {
        public String Guid { get; }

        public DeconstructionBegin(String playerId, Vector3 itemPosition, String guid) : base(playerId, itemPosition)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionBegin( - playerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
