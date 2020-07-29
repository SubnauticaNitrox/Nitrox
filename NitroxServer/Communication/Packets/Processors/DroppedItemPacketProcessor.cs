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

        public override void Process(DroppedItem packet, Player player)
        {
            bool existsInGlobalRoot = Map.Main.GlobalRootTechTypes.Contains(packet.TechType);
            NitroxObject obj = new NitroxObject(packet.Id);
            obj.Transform.Position = packet.ItemPosition;
            obj.Transform.Rotation = packet.ItemRotation;

            Entity entity = new Entity(packet.TechType, 0, null, true, packet.WaterParkId.OrElse(null), packet.Bytes, existsInGlobalRoot);
            obj.AddBehavior(entity);
            entityManager.RegisterNewEntity(entity);

            SimulatedEntity simulatedEntity = entitySimulation.AssignNewEntityToPlayer(entity, player);

            SimulationOwnershipChange ownershipChangePacket = new SimulationOwnershipChange(simulatedEntity);
            playerManager.SendPacketToAllPlayers(ownershipChangePacket);

            foreach (Player connectedPlayer in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != connectedPlayer;
                if (isOtherPlayer && connectedPlayer.CanSee(entity))
                {
                    CellEntities cellEntities = new CellEntities(entity);
                    connectedPlayer.SendPacket(cellEntities);
                }
            }
        }
    }
}
