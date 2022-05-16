using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxServer.Communication.LiteNetLib
{
    public class LiteNetLibConnection : NitroxConnection
    {
        private readonly NetPacketProcessor netPacketProcessor = new();
        private readonly NetPeer peer;

        public IPEndPoint Endpoint => peer.EndPoint;
        public NitroxConnectionState State => MapConnectionState(peer.ConnectionState);

        private NitroxConnectionState MapConnectionState(ConnectionState connectionState)
        {
            NitroxConnectionState state = NitroxConnectionState.Unknown;

            if (connectionState.HasFlag(ConnectionState.Connected))
            {
                state = NitroxConnectionState.Connected;
            }

            if (connectionState.HasFlag(ConnectionState.Disconnected))
            {
                state = NitroxConnectionState.Disconnected;
            }

            return state;
        }

        public LiteNetLibConnection(NetPeer peer)
        {
            this.peer = peer;
        }

        public static bool operator ==(LiteNetLibConnection left, LiteNetLibConnection right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LiteNetLibConnection left, LiteNetLibConnection right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((LiteNetLibConnection)obj);
        }

        public override int GetHashCode()
        {
            return peer?.Id.GetHashCode() ?? 0;
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
                Log.Warn($"Cannot send packet {packet?.GetType()} to a closed connection {peer?.EndPoint}");
            }
        }

        protected bool Equals(LiteNetLibConnection other)
        {
            return peer?.Id == other.peer?.Id;
        }
    }
}
