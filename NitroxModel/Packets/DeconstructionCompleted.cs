using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionCompleted : Packet
    {
        public string Guid { get; }

        public DeconstructionCompleted(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionCompleted Guid: " + Guid + "]";
        }
    }
}
