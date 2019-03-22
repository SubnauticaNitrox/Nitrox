using Lidgren.Network;
using NitroxModel.Logger;
using NitroxModel.Networking;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace NitroxServer.Communication.NetworkingLayer.Lidgren
{
    public class LidgrenConnection : NitroxConnection
    {
        private readonly NetServer server;
        private readonly NetConnection connection;

        public LidgrenConnection(NetServer server, NetConnection connection)
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

                connection.SendMessage(om, NitroxDeliveryMethod.ToLidgren(packet.DeliveryMethod), (int)packet.UdpChannel);
            }
            else
            {
                Log.Info("Cannot send packet to a closed connection.");
            }
        }
    }
}
