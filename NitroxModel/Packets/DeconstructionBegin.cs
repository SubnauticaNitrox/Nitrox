using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionBegin : RangedPacket
    {
        public string Guid { get; }

        public DeconstructionBegin(Vector3 itemPosition, string guid) : base(itemPosition, 3)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionBegin Guid: " + Guid + "]";
        }
    }
}
