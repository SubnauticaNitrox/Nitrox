using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NitroxServerUdpPunch.Communication.NetworkingLayer
{
    internal struct ServerIpInfo
    {
        private IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;
        DateTime lastUpdated;

        internal ServerIpInfo(IPEndPoint local, IPEndPoint remote)
        {
            localEndPoint = local;
            remoteEndPoint = remote;
            lastUpdated = DateTime.Now;
        }

        public IPEndPoint LocalEndPoint { get => localEndPoint;}
        public IPEndPoint RemoteEndPoint { get => remoteEndPoint; }
        public DateTime LastUpdated { get => lastUpdated; }

        internal void UpdateEndpoints(IPEndPoint local, IPEndPoint remote)
        {
            localEndPoint = local;
            remoteEndPoint = remote;
            lastUpdated = DateTime.Now;
        }
    }
}
