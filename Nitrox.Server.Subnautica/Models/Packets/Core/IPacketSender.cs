using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

internal interface IPacketSender
{
    /// <summary>
    ///     Sends a packet to the given session id, if still connected.
    /// </summary>
    ValueTask SendPacket<T>(T packet, SessionId sessionId) where T : Packet;
    ValueTask SendPacketToAll<T>(T packet) where T : Packet;
    ValueTask SendPacketToOthers<T>(T packet, SessionId excludedSessionId) where T : Packet;
}
