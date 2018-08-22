using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using LiteNetLib;

namespace NitroxServer.Communication
{
    public class Connection : IProcessorContext
    {
        private readonly NetPeer peer;

        public Connection(NetPeer peer)
        {
            this.peer = peer;
        }
        
        public void SendPacket(Packet packet)
        {
            if (peer.ConnectionState == ConnectionState.Connected)
            {
                byte[] packetData = packet.Serialize();
                peer.Send(packetData, packet.DeliveryMethod);
                peer.Flush();
            }
            else
            {
                Log.Info("Cannot send packet to a closed connection.");
            }
        }
    }
}
