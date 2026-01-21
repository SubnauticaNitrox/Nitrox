using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Core;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.Packets.Processors.Core;

/// <summary>
///     Context used by <see cref="IAuthPacketProcessor{TPacket}" />.
/// </summary>
public record ClientProcessorContext : IPacketProcessContext<Player>
{
    private readonly IPacketSender packetSender;
    public Player Sender { get; set; }

    public ClientProcessorContext(Player sender, IPacketSender packetSender)
    {
        this.packetSender = packetSender;
        Sender = sender;
    }

    public void Send<T>(T packet) where T : Packet => packetSender.Send(packet);
}
