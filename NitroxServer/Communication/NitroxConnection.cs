using System.Net;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace NitroxServer.Communication
{
    public interface NitroxConnection : IProcessorContext
    {
        IPEndPoint Endpoint { get; }
        void SendPacket(Packet packet);
    }
}
