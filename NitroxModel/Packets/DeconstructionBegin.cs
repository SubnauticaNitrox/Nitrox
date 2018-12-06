using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DeconstructionBegin : Packet
    {
        public string Guid { get; }

        public DeconstructionBegin(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[DeconstructionBegin Guid: " + Guid + "]";
        }
    }
}
