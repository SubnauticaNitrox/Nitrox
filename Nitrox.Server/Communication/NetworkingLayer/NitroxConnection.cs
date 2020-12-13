using System.Net;
using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Processors.Abstract;

namespace Nitrox.Server.Communication.NetworkingLayer
{
    public interface NitroxConnection : IProcessorContext
    {
        IPEndPoint Endpoint { get; }
        void SendPacket(Packet packet);
    }
}
