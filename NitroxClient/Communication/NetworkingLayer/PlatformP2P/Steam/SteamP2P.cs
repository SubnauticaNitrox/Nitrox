using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using Steamworks;

namespace NitroxClient.Communication.NetworkingLayer.PlatformP2P.Steam
{
    public class SteamP2P : IConnectionInfo
    {
        private CSteamID host;

        public SteamP2P(CSteamID host)
        {
            this.host = host;
        }

        public override bool Equals(object obj)
        {
            return obj is SteamP2P p &&
                   host.Equals(p.host);
        }

        public bool Equals(IConnectionInfo other)
        {
            return other is SteamP2P steamP2P && host.Equals(steamP2P.host);
        }

        public override int GetHashCode()
        {
            return 898718151 + host.m_SteamID.GetHashCode();
        }

        public static bool operator ==(SteamP2P left, SteamP2P right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SteamP2P left, SteamP2P right)
        {
            return !(left == right);
        }
    }
}
