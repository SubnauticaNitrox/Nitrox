using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Test.Client.Communication;

[Serializable]
public class TestNonActionPacket(SessionId sessionId) : Packet
{
    public SessionId SessionId { get; } = sessionId;
}
