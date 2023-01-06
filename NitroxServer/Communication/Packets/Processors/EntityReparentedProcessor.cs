using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    public class EntityReparentedProcessor : AuthenticatedPacketProcessor<EntityReparented>
    {
        private readonly EntityRegistry entityRegistry;

        public EntityReparentedProcessor(EntityRegistry entityRegistry)
        {
            this.entityRegistry = entityRegistry;
        }

        public override void Process(EntityReparented packet, Player player)
        {
            Optional<Entity> opEntity = entityRegistry.GetEntityById(packet.Id);

            if (!opEntity.HasValue)
            {
                Log.Info($"Could not find entity to reparent: {packet}");
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
                Log.Info($"Could not find new parent to reparent an entity for: {packet}");
            }
        }
    }
}
