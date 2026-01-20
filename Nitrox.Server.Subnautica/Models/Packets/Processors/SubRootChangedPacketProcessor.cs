using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SubRootChangedPacketProcessor : AuthenticatedPacketProcessor<SubRootChanged>
{
    private readonly IPacketSender packetSender;
    private readonly EntityRegistry entityRegistry;

    public SubRootChangedPacketProcessor(IPacketSender packetSender, EntityRegistry entityRegistry)
    {
        this.packetSender = packetSender;
        this.entityRegistry = entityRegistry;
    }

    public override void Process(SubRootChanged packet, Player player)
    {
        entityRegistry.ReparentEntity(player.GameObjectId, packet.SubRootId.OrNull());
        player.SubRootId = packet.SubRootId;
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
