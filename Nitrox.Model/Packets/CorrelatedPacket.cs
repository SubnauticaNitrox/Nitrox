using System;

namespace Nitrox.Model.Packets;

// TODO: Refactor this away. Use SessionId instead of correlationId.
[Serializable]
public abstract class CorrelatedPacket(string correlationId) : Packet
{
    public string CorrelationId { get; protected set; } = correlationId;
}
