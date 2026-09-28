using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

internal interface IPacketSender
{
    /// <summary>
    ///     Sends a packet to the given session id, if still connected.
    /// </summary>
    ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet;

    /// <summary>
    ///     Sends a packet to all connected sessions.
    /// </summary>
    /// <typeparam name="T">The type of packet to send.</typeparam>
    /// <param name="packet">The packet to send.</param>
    ValueTask SendPacketToAllAsync<T>(T packet) where T : Packet;

    /// <summary>
    ///     Sends a packet to all connected sessions except the specified session.
    /// </summary>
    /// <typeparam name="T">The type of packet to send.</typeparam>
    /// <param name="packet">The packet to send.</param>
    /// <param name="excludedSessionId">The session ID to exclude from receiving the packet.</param>
    ValueTask SendPacketToOthersAsync<T>(T packet, SessionId excludedSessionId) where T : Packet;
}
