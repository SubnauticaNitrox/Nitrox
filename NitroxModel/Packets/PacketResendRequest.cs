using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PacketResendRequest : Packet
    {
        //public int Sequence { get; }

        public PacketResendRequest(long sequence)
        {
            Sequence = sequence;
        }

        public override string ToString()
        {
            return "[PacketResendRequest Sequence:" + Sequence + "]";
        }
    }
}
