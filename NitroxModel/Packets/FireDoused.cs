using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FireDoused : Packet
    {
        public NitroxId Id { get; }
        public float DouseAmount { get; }

        public FireDoused(NitroxId id, float douseAmount)
        {
            Id = id;
            DouseAmount = douseAmount;
        }

        public override string ToString()
        {
            return $"[FireDoused - Id: {Id}, DouseAmount: {DouseAmount}]";
        }
    }
}
