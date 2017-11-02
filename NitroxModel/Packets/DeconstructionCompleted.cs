using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : PlayerActionPacket
    {
        public string Guid { get; }

        public DeconstructionCompleted(Vector3 itemPosition, string guid) : base(itemPosition)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionCompleted Guid: " + Guid + "]";
        }
    }
}
