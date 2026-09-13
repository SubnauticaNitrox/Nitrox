using Nitrox.Model.Core;
using Nitrox.Model.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

/// <summary>
///     Context used by <see cref="IAuthPacketProcessor{TPacket}" />.
/// </summary>
internal record AuthProcessorContext : IPacketProcessContext<SessionId>
{
    private readonly IPacketSender packetSender;
    public SessionId Sender { get; set; }

    public AuthProcessorContext(SessionId senderId, IPacketSender packetSender)
    {
        this.packetSender = packetSender;
        Sender = senderId;
    }

    public async Task SendAsync<T>(T packet, SessionId sessionId) where T : Packet => await packetSender.SendPacketAsync(packet, sessionId);

    public async Task ReplyAsync<T>(T packet) where T : Packet => await packetSender.SendPacketAsync(packet, Sender);

    public async Task SendToAllAsync<T>(T packet) where T : Packet => await packetSender.SendPacketToAllAsync(packet);

    public async Task SendToOthersAsync<T>(T packet) where T : Packet => await packetSender.SendPacketToOthersAsync(packet, Sender);

    public override string ToString() => $"#{Sender}";
}
