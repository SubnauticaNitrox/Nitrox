using System;
using System.Runtime.Serialization;

namespace NitroxModel.Packets.Exceptions
{
    [Serializable]
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

        protected UncorrelatedPacketException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            InvalidPacket = (CorrelatedPacket)info.GetValue("invalidPacket", typeof(CorrelatedPacket));
            ExpectedCorrelationId = (string)info.GetValue("expectedCorrelationId", typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("invalidPacket", InvalidPacket);
            info.AddValue("expectedCorrelationId", ExpectedCorrelationId);
        }
    }
}
