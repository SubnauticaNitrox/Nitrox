using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class DroppedItemPacketProcessor : AuthenticatedPacketProcessor<DroppedItem>
    {
        private readonly WorldEntityManager worldEntityManager;
        private readonly PlayerManager playerManager;
        private readonly EntitySimulation entitySimulation;
        private readonly IMap map;

        public DroppedItemPacketProcessor(WorldEntityManager worldEntityManager, PlayerManager playerManager, EntitySimulation entitySimulation, IMap map)
        {
            this.worldEntityManager = worldEntityManager;
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;
            this.map = map;
        }

        public override void Process(DroppedItem packet, Player droppingPlayer)
        {
            bool existsInGlobalRoot = map.GlobalRootTechTypes.Contains(packet.TechType);

            WorldEntity entity = new WorldEntity(packet.ItemPosition, packet.ItemRotation, NitroxVector3.One, packet.TechType, 0, null, false, packet.Id, null, existsInGlobalRoot, packet.WaterParkId.OrNull());
            worldEntityManager.RegisterNewEntity(entity);

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
