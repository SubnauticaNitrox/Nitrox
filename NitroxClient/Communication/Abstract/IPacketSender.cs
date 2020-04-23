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

        PacketSuppressor<T> Suppress<T>();
    }
}
