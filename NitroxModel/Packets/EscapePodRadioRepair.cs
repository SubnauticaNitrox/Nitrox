using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EscapePodRadioRepair : Packet
    {
        public string Guid { get; }

        public EscapePodRadioRepair(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[EscapePodRadioRepair guid: " + Guid + "]";
        }
    }
}
