using System;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract;

public interface IPacketSender
{
    PacketSuppressor<T> Suppress<T>();

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
