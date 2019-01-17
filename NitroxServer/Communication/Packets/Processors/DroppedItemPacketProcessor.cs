using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class DroppedItemPacketProcessor : AuthenticatedPacketProcessor<DroppedItem>
    {
        private readonly EntityManager entityManager;
        private readonly PlayerManager playerManager;
        private readonly EntitySimulation entitySimulation;

        public DroppedItemPacketProcessor(EntityManager entityManager, PlayerManager playerManager, EntitySimulation entitySimulation)
        {
            this.entityManager = entityManager;
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;
        }

        public override void Process(DroppedItem packet, Player droppingPlayer)
        {
            bool existsInGlobalRoot = Map.GLOBAL_ROOT_TECH_TYPES.Contains(packet.TechType);
            Entity entity = new Entity(packet.ItemPosition, packet.ItemRotation, new UnityEngine.Vector3(1, 1, 1), packet.TechType, 0, null, true, packet.WaterParkGuid.OrElse(null), packet.Bytes, existsInGlobalRoot, packet.Guid);
            entityManager.RegisterNewEntity(entity);

            SimulatedEntity simulatedEntity = entitySimulation.AssignNewEntityToPlayer(entity, droppingPlayer);

            SimulationOwnershipChange ownershipChangePacket = new SimulationOwnershipChange(simulatedEntity);
            playerManager.SendPacketToAllPlayers(ownershipChangePacket);

            foreach (Player player in playerManager.GetPlayers())
            {
                bool isOtherPlayer = (player != droppingPlayer);

                if(isOtherPlayer && player.CanSee(entity))
                {
                    CellEntities cellEntities = new CellEntities(entity);
                    player.SendPacket(cellEntities);
                }
            }
        }
    }
}
