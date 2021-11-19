using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public interface IPacketSender
    {
        /// <summary>
        ///     Sends the packet. Returns true if packet was not suppressed.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <returns>True if not suppressed and actually sent.</returns>
        bool Send(Packet packet);
        /// <summary>
        ///     Saves only the newest <see cref="Packet"/> per type in a list. This list can be send with <see cref="FlushSmoothPackets"/>.
        /// </summary>
        bool SendSmooth(Packet packet);
        /// <summary>
        ///     Sends all packets queued by <see cref="SendSmooth(Packet)"/>.
        /// </summary>
        void FlushSmoothPackets();

        PacketSuppressor<T> Suppress<T>();
    }
}
