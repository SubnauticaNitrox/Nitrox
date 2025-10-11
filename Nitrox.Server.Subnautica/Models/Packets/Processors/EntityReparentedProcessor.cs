using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class EntityReparentedProcessor : AuthenticatedPacketProcessor<EntityReparented>
{
    private readonly EntityRegistry entityRegistry;
    private readonly PlayerManager playerManager;

    public EntityReparentedProcessor(EntityRegistry entityRegistry, PlayerManager playerManager)
    {
        this.entityRegistry = entityRegistry;
        this.playerManager = playerManager;
    }

    public override void Process(EntityReparented packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            Log.Error($"Couldn't find entity for {packet.Id}");
            return;
        }
        if (!entityRegistry.TryGetEntityById(packet.NewParentId, out Entity parentEntity))
        {
            Log.Error($"Couldn't find parent entity for {packet.NewParentId}");
            return;
        }
        
        entityRegistry.ReparentEntity(packet.Id, packet.NewParentId);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
