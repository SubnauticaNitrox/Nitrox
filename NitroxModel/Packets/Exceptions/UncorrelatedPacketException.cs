using System;
using System.Runtime.Serialization;

namespace NitroxModel.Packets.Exceptions
{
    public class UncorrelatedPacketException : Exception
    {
        public CorrelatedPacket InvalidPacket { get; }
        public string ExpectedCorrelationId { get; }

        public UncorrelatedPacketException(CorrelatedPacket invalidPacket, string expectedCorrelationId)
        {
            InvalidPacket = invalidPacket;
            ExpectedCorrelationId = expectedCorrelationId;
        }

        public UncorrelatedPacketException(string message, CorrelatedPacket invalidPacket, string expectedCorrelationId) : base(message)
        {
            InvalidPacket = invalidPacket;
            ExpectedCorrelationId = expectedCorrelationId;
        }

        public UncorrelatedPacketException(string message, Exception innerException, CorrelatedPacket invalidPacket, string expectedCorrelationId) : base(message, innerException)
        {
            InvalidPacket = invalidPacket;
            ExpectedCorrelationId = expectedCorrelationId;
        }
    }
}
