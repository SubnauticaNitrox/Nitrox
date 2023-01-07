using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

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
        Optional<Entity> opEntity = entityRegistry.GetEntityById(packet.Id);

        if (!opEntity.HasValue)
        {
            Log.Error($"Could not find entity to reparent: {packet}");
            return;
        }

        Entity entity = opEntity.Value;
        entity.ParentId = packet.NewParentId;

        Optional<Entity> oldParent = entityRegistry.GetEntityById(entity.ParentId);

        // old parent may not exist anymore, so don't expect it.
        if (oldParent.HasValue)
        {
            oldParent.Value.ChildEntities.Remove(entity);
        }

        Optional<Entity> newParent = entityRegistry.GetEntityById(packet.NewParentId);

        // old parent may not exist anymore, so don't expect it.
        if (newParent.HasValue)
        {
            newParent.Value.ChildEntities.Add(entity);
        }
        else
        {
            Log.Error($"Could not find new parent to reparent an entity for: {packet}");
        }

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
