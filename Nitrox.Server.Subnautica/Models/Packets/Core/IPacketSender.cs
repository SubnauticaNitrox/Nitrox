using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

internal interface IPacketSender
{
    /// <summary>
    ///     Sends a packet to the given session id, if still connected.
    /// </summary>
    ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet;
    ValueTask SendPacketToAllAsync<T>(T packet) where T : Packet;
    ValueTask SendPacketToOthersAsync<T>(T packet, SessionId excludedSessionId) where T : Packet;
}
