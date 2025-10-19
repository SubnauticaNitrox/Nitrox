using System.Net;
using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Processors.Abstract;

namespace Nitrox.Server.Subnautica.Models.Communication;

public interface INitroxConnection : IProcessorContext
{
    IPEndPoint Endpoint { get; }

    NitroxConnectionState State { get; }

    void SendPacket(Packet packet);
}
