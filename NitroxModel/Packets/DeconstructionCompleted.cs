using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : RangedPacket
    {
        public string Guid { get; }

        public DeconstructionCompleted(Vector3 itemPosition, string guid) : base(itemPosition, 3)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionCompleted Guid: " + Guid + "]";
        }
    }
}
