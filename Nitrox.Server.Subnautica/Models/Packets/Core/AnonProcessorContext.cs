using Nitrox.Model.Core;
using Nitrox.Model.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

/// <summary>
///     Context used by <see cref="IAuthPacketProcessor{TPacket}" />.
/// </summary>
internal record AnonProcessorContext : IPacketProcessContext<SessionId>
{
    private readonly IPacketSender packetSender;
    public SessionId Sender { get; set; }

    public AnonProcessorContext(SessionId sender, IPacketSender packetSender)
    {
        this.packetSender = packetSender;
        Sender = sender;
    }

    public async Task ReplyToSender<T>(T packet) where T : Packet => await packetSender.SendPacketAsync(packet, Sender);

    public async Task ReplyToAll<T>(T packet) where T : Packet => await packetSender.SendPacketToAllAsync(packet);
}
