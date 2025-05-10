using System;
using System.Net;
using NitroxModel.Networking.Packets;

namespace NitroxServer.Communication;

[Obsolete("Use SessionId for sending packets and getting the endpoint data from database")]
public interface INitroxConnection
{
    IPEndPoint Endpoint { get; }

    NitroxConnectionState State { get; }

    void SendPacket(Packet packet);
}
