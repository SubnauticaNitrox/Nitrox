using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : DeferrablePacket
    {
        public string Guid { get; }

        public DeconstructionCompleted(Vector3 itemPosition, string guid) : base(itemPosition, BUILDING_CELL_LEVEL)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionCompleted Guid: " + Guid + "]";
        }
    }
}
