using System;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public interface IPacketSender
    {
        /// <summary>
        ///     Sends the packet. Returns true if packet was not suppressed.
        /// </summary>
        void Send<T>(T packet) where T : Packet;

        /// <summary>
        ///     Sends the packet if it originated from UWE code (or if patched in), unless game code was called by Nitrox.
        /// </summary>
        bool SendIfGameCode<T>(T packet) where T : Packet;
        
        bool IsPacketSuppressed(Type packetType);

        PacketSuppressor<T> Suppress<T>();
    }
}
