using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace NitroxServer.Communication
{
    public class Connection : IProcessorContext
    {
        private readonly NetPeer peer;
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();

        public Connection(NetPeer peer)
        {
            this.peer = peer;
        }

        public void SendPacket(Packet packet)
        {
            if (peer.ConnectionState == ConnectionState.Connected)
            {
                peer.Send(netPacketProcessor.Write(packet.toWrapperPacket()), packet.DeliveryMethod);
                peer.Flush();
            }
            else
            {
                Log.Info("Cannot send packet to a closed connection.");
            }
        }
    }
}
