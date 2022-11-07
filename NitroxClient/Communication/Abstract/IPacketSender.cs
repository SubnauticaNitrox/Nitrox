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
        ///     Sends the packet if the call stack originated from UWE code.
        ///     If the game code was called by Nitrox, triggering a packet send in injected code, no packet is send.
        /// </summary>
        bool SendIfGameCode<T>(T packet) where T : Packet;
    }
}
