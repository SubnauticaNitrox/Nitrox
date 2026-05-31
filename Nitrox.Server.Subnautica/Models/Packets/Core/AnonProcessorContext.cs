using System.Net;
using Nitrox.Model.Core;
using Nitrox.Model.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

/// <summary>
///     Context used by <see cref="IAuthPacketProcessor{TPacket}" />.
/// </summary>
internal record AnonProcessorContext : IPacketProcessContext<(SessionId SessionId, IPEndPoint EndPoint)>
{
    private readonly IPacketSender packetSender;
    public (SessionId SessionId, IPEndPoint EndPoint) Sender { get; set; }

    public AnonProcessorContext((SessionId SessionId, IPEndPoint EndPoint) sender, IPacketSender packetSender)
    {
        this.packetSender = packetSender;
        Sender = sender;
    }

    public async Task ReplyAsync<T>(T packet) where T : Packet => await packetSender.SendPacketAsync(packet, Sender.SessionId);

    public async Task SendToAllAsync<T>(T packet) where T : Packet => await packetSender.SendPacketToAllAsync(packet);
}
