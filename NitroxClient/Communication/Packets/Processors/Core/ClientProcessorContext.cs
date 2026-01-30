using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Core;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.Packets.Processors.Core;

public record ClientProcessorContext : IPacketProcessContext
{
    private readonly IPacketSender packetSender;

    public ClientProcessorContext(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public void Send<T>(T packet) where T : Packet => packetSender.Send(packet);
}
