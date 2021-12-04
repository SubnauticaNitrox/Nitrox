using System.Net;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Networking;
using System;

namespace NitroxServer.Communication
{
    public interface INitroxConnection : IProcessorContext
    {
        IConnectionInfo Endpoint { get; }
        void SendPacket(Packet packet);

        void Disconnect();
    }
}
