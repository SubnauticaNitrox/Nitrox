using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : PlayerActionPacket
    {
        public string Guid { get; }

        public DeconstructionCompleted(string playerId, Vector3 itemPosition, string guid) : base(playerId, itemPosition)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionCompleted( - playerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
