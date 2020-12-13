using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Entities;

namespace Nitrox.Server.Communication.Packets.Processors
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
            bool existsInGlobalRoot = Map.Main.GlobalRootTechTypes.Contains(packet.TechType);
            Entity entity = new Entity(packet.ItemPosition, packet.ItemRotation, NitroxVector3.One, packet.TechType, 0, null, true, packet.WaterParkId.OrElse(null), packet.Bytes, existsInGlobalRoot, packet.Id);
            entityManager.RegisterNewEntity(entity);

            SimulatedEntity simulatedEntity = entitySimulation.AssignNewEntityToPlayer(entity, droppingPlayer);

            SimulationOwnershipChange ownershipChangePacket = new SimulationOwnershipChange(simulatedEntity);
            playerManager.SendPacketToAllPlayers(ownershipChangePacket);

            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != droppingPlayer;
                if (isOtherPlayer && player.CanSee(entity))
                {
                    CellEntities cellEntities = new CellEntities(entity);
                    player.SendPacket(cellEntities);
                }
            }
        }
    }
}
