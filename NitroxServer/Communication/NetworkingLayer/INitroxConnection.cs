using System.Collections.Generic;
using System.Net;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.NetworkingLayer.Tunnel;

namespace NitroxServer.Communication.NetworkingLayer
{
    public interface INitroxConnection : IProcessorContext
    {
        IPEndPoint Endpoint { get; }
        HashSet<TunneledConnection> TunneledConnections { get; }

        void SendPacket(Packet packet);

        void Disconnect();
    }
}
