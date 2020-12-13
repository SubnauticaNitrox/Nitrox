using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using Nitrox.Model.Logger;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace Nitrox.Server.Communication.NetworkingLayer.LiteNetLib
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
                Log.Info("Cannot send packet to a closed connection.");
            }
        }
    }
}
