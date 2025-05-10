using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

/// <summary>
///     Context used by <see cref="IAuthPacketProcessor{TPacket}" />.
/// </summary>
internal record AuthProcessorContext : IPacketProcessContext<(PeerId PlayerId, SessionId SessionId)>
{
    private readonly IServerPacketSender packetSender;
    public (PeerId PlayerId, SessionId SessionId) Sender { get; set; }

    public AuthProcessorContext((PeerId PlayerId, SessionId SessionId) sender, IServerPacketSender packetSender)
    {
        this.packetSender = packetSender;
        Sender = sender;
    }

    public async Task Send<T>(T packet, SessionId sessionId) where T : Packet => await packetSender.SendPacket(packet, sessionId);

    public async Task ReplyToSender<T>(T packet) where T : Packet => await packetSender.SendPacket(packet, Sender.SessionId);

    public async Task ReplyToAll<T>(T packet) where T : Packet => await packetSender.SendPacketToAll(packet);

    public async Task ReplyToOthers<T>(T packet) where T : Packet => await packetSender.SendPacketToOthers(packet, Sender.SessionId);
}
