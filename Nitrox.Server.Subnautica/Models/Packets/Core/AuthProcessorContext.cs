using Nitrox.Model.Core;
using Nitrox.Model.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

/// <summary>
///     Context used by <see cref="IAuthPacketProcessor{TPacket}" />.
/// </summary>
internal record AuthProcessorContext : IPacketProcessContext<Player>
{
    private readonly IPacketSender packetSender;
    public Player Sender { get; set; }

    public AuthProcessorContext(Player sender, IPacketSender packetSender)
    {
        this.packetSender = packetSender;
        Sender = sender;
    }

    public async Task SendAsync<T>(T packet, SessionId sessionId) where T : Packet => await packetSender.SendPacketAsync(packet, sessionId);

    public async Task ReplyToSender<T>(T packet) where T : Packet => await packetSender.SendPacketAsync(packet, Sender.SessionId);

    public async Task ReplyToAll<T>(T packet) where T : Packet => await packetSender.SendPacketToAllAsync(packet);

    public async Task ReplyToOthers<T>(T packet) where T : Packet => await packetSender.SendPacketToOthersAsync(packet, Sender.SessionId);
}
