using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class EntityReparentedProcessor(EntityRegistry entityRegistry, PlayerManager playerManager, ILogger<EntityReparentedProcessor> logger) : AuthenticatedPacketProcessor<EntityReparented>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerManager playerManager = playerManager;
    private readonly ILogger<EntityReparentedProcessor> logger = logger;

    public override void Process(EntityReparented packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            logger.ZLogError($"Couldn't find entity for {packet.Id}");
            return;
        }
        if (!entityRegistry.TryGetEntityById(packet.NewParentId, out Entity parentEntity))
        {
            logger.ZLogError($"Couldn't find parent entity for {packet.NewParentId}");
            return;
        }

        entityRegistry.ReparentEntity(packet.Id, packet.NewParentId);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
