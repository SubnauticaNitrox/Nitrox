using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

/// <summary>
///     Context used by <see cref="IAuthPacketProcessor{TPacket}" />.
/// </summary>
internal record AnonProcessorContext : IPacketProcessContext<SessionId>
{
    private readonly IServerPacketSender packetSender;
    public SessionId Sender { get; set; }

    public AnonProcessorContext(SessionId sender, IServerPacketSender packetSender)
    {
        this.packetSender = packetSender;
        Sender = sender;
    }

    public async Task ReplyToSender<T>(T packet) where T : Packet => await packetSender.SendPacket(packet, Sender);

    public async Task ReplyToAll<T>(T packet) where T : Packet => await packetSender.SendPacketToAll(packet);
}
