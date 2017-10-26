using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionBegin : PlayerActionPacket
    {
        public string Guid { get; }

        public DeconstructionBegin(string playerId, Vector3 itemPosition, string guid) : base(playerId, itemPosition)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionBegin( - playerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
