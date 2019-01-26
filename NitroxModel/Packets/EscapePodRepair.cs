using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EscapePodRepair : Packet
    {
        public string Guid { get; }

        public EscapePodRepair(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[EscapePodRepair guid: " + Guid + "]";
        }
    }
}
