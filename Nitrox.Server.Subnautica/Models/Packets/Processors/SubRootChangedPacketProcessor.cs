using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class SubRootChangedPacketProcessor(EntityRegistry entityRegistry) : IAuthPacketProcessor<SubRootChanged>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, SubRootChanged packet)
    {
        // TODO: USE DATABASE
        // entityRegistry.ReparentEntity(player.GameObjectId, packet.SubRootId.OrNull());
        // player.SubRootId = packet.SubRootId;
        // playerService.SendPacketToOtherPlayers(packet, player);
    }
}
