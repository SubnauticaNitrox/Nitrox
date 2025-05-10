using System;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public abstract record CorrelatedPacket : Packet
    {
        public string CorrelationId { get; protected set; }

        protected CorrelatedPacket(string correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
