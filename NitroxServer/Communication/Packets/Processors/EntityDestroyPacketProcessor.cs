using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    public class EntityDestroyPacketProcessor : AuthenticatedPacketProcessor<EntityDestroy>
    {
        private readonly EntityManager entityManager;

        public EntityDestroyPacketProcessor(EntityManager entityManager)
        {
            this.entityManager = entityManager;
        }

        public override void Process(EntityDestroy packet, Player player)
        {
            entityManager.RemoveEntity(packet.EntityId);
        }
    }
}
