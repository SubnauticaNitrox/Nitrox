using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Logger;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxServer.Communication.NetworkingLayer.LiteNetLib
{
    public class LiteNetLibConnection : NitroxConnection
    {
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private readonly NetPeer peer;

        public IPEndPoint Endpoint => peer.EndPoint;

        public LiteNetLibConnection(NetPeer peer)
        {
            this.peer = peer;
        }

        public void SendPacket(Packet packet)
        {
            if (peer.ConnectionState == ConnectionState.Connected)
            {
                peer.Send(netPacketProcessor.Write(packet.ToWrapperPacket()), NitroxDeliveryMethod.ToLiteNetLib(packet.DeliveryMethod));
                peer.Flush();
            }
            else
            {
                Log.Instance.LogMessage(LogCategory.Info, "Cannot send packet to a closed connection.");
            }
        }
    }
}
