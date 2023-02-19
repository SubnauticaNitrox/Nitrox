using System;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract;

public interface IPacketSender
{
    [Obsolete($"Use {nameof(IPacketSender)}.{nameof(SendIfGameCode)} instead. If already done then this call can be removed as it's handled by {nameof(SendIfGameCode)}.")]
    PacketSuppressor<T> Suppress<T>();

    /// <summary>
    ///     This shouldn't be used anymore.
    ///     Packet suppression is done via <see cref="SendIfGameCode{T}" /> and will automatically suppress when appropriate using stack trace.
    /// </summary>
    /// <param name="packetType"></param>
    /// <returns></returns>
    bool IsPacketSuppressed(Type packetType);

    /// <summary>
    ///     Sends the packet.
    /// </summary>
    void Send<T>(T packet) where T : Packet;

    /// <summary>
    ///     Sends the packet if the call stack originated from UWE code.
    ///     If the game code was called by Nitrox, triggering a packet send in injected code, no packet is send.
    /// </summary>
    bool SendIfGameCode<T>(T packet) where T : Packet;
}
