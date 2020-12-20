using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxServer.Communication.NetworkingLayer.Tunnel
{
    public class TunneledConnection : INitroxConnection, IEquatable<TunneledConnection>
    {
        public IPEndPoint Endpoint => Host.Endpoint;
        internal int hostRelativeId { get; private set; }

        public HashSet<TunneledConnection> TunneledConnections { get; }

        public INitroxConnection Host { get; private set; }

        public TunneledConnection(INitroxConnection host)
        {
            TunneledConnections = new HashSet<TunneledConnection>();
            UpdateHost(host);
        }

        public void UpdateHost(INitroxConnection newHost)
        {
            if (Host != null)
            {
                throw new NotImplementedException("TODO");
            }

            Host = newHost;
            hostRelativeId = Host.TunneledConnections.Count;
            Host.TunneledConnections.Add(this);
        }

        public void SendPacket(Packet packet)
        {
            Host.SendPacket(new TunneledPacket(packet, hostRelativeId));
        }

        public void Disconnect()
        {
            Host.TunneledConnections.Remove(this);
            int i = 0;
            foreach (TunneledConnection tunneledConnection in Host.TunneledConnections)
            {
                tunneledConnection.hostRelativeId = i++;
            }

            foreach (TunneledConnection tunneledConnection in TunneledConnections)
            {
                tunneledConnection.Disconnect();
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TunneledConnection);
        }

        public bool Equals(TunneledConnection other)
        {
            return other != null &&
                   EqualityComparer<IPEndPoint>.Default.Equals(Endpoint, other.Endpoint) &&
                   hostRelativeId == other.hostRelativeId;
        }

        public override int GetHashCode()
        {
            int hashCode = 1488984782;
            hashCode = hashCode * -1521134295 + EqualityComparer<IPEndPoint>.Default.GetHashCode(Endpoint);
            hashCode = hashCode * -1521134295 + hostRelativeId.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(TunneledConnection left, TunneledConnection right)
        {
            return EqualityComparer<TunneledConnection>.Default.Equals(left, right);
        }

        public static bool operator !=(TunneledConnection left, TunneledConnection right)
        {
            return !(left == right);
        }
    }
}
