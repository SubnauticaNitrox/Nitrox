using System.Collections.Generic;
using System.Net;
using NitroxModel.Networking;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.NetworkingLayer.Tunnel;

namespace NitroxServer.Communication.NetworkingLayer
{
    public interface INitroxConnection : IProcessorContext
    {
        IConnectionInfo Endpoint { get; }
        HashSet<TunneledConnection> TunneledConnections { get; }

        void SendPacket(Packet packet);
        void Disconnect();
    }
}
