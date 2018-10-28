using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using Lidgren.Network;

namespace NitroxServer.Communication
{
    public class Connection : IProcessorContext
    {
        private readonly NetServer server;
        private readonly NetConnection connection;

        public Connection(NetServer server, NetConnection connection)
        {
            this.server = server;
            this.connection = connection;
        }
        
        public void SendPacket(Packet packet)
        {
            if (connection.Status == NetConnectionStatus.Connected)
            {
                byte[] packetData = packet.Serialize();
                NetOutgoingMessage om = server.CreateMessage();
                om.Write(packetData);

                connection.SendMessage(om, packet.DeliveryMethod, (int)packet.UdpChannel);
            }
            else
            {
                StaticLogger.Instance.Info("Cannot send packet to a closed connection.");
            }
        }
    }
}
