using System;
using Nitrox.Model.Core;

namespace Nitrox.Model.Packets.Exceptions;

public sealed class UncorrelatedPacketException(Packet invalidPacket, SessionId expectedSessionId) : Exception
{
    public Packet InvalidPacket { get; } = invalidPacket;
    public SessionId ExpectedSessionId { get; } = expectedSessionId;
}
