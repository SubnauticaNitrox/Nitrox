using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    public class EntityDestroyPacketProcessor : AuthenticatedPacketProcessor<EntityDestroy>
    {
        private readonly EntityRegistry entityRegistry;

        public EntityDestroyPacketProcessor(EntityRegistry entityRegistry)
        {
            this.entityRegistry = entityRegistry;
        }

        public override void Process(EntityDestroy packet, Player player)
        {
            entityRegistry.RemoveEntity(packet.EntityId);
        }
    }
}
