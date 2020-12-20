using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Logger;
using NitroxModel.Networking;
using NitroxModel.Packets;
using NitroxServer.Communication.NetworkingLayer.Tunnel;

namespace NitroxServer.Communication.NetworkingLayer.LiteNetLib
{
    public class LiteNetLibConnection : INitroxConnection
    {
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private readonly NetPeer peer;

        public HashSet<TunneledConnection> TunneledConnections { get; }

        public IPEndPoint Endpoint => peer.EndPoint;

        public LiteNetLibConnection(NetPeer peer)
        {
            this.peer = peer;
            TunneledConnections = new HashSet<TunneledConnection>();
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

        public void Disconnect()
        {
            foreach (TunneledConnection tunneledConnection in TunneledConnections)
            {
                tunneledConnection.Disconnect();
            }

            peer.Disconnect();
        }
    }
}
