using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionBegin : PlayerActionPacket
    {
        public string Guid { get; }

        public DeconstructionBegin(Vector3 itemPosition, string guid) : base(itemPosition)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionBegin Guid: " + Guid + "]";
        }
    }
}
