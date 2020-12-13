using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Entities;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class PickupItemPacketProcessor : AuthenticatedPacketProcessor<PickupItem>
    {
        private readonly EntityManager entityManager;
        private readonly PlayerManager playerManager;
        private readonly SimulationOwnershipData simulationOwnershipData;

        public PickupItemPacketProcessor(EntityManager entityManager, PlayerManager playerManager, SimulationOwnershipData simulationOwnershipData)
        {
            this.entityManager = entityManager;
            this.playerManager = playerManager;
            this.simulationOwnershipData = simulationOwnershipData;
        }

        public override void Process(PickupItem packet, Player player)
        {
            if(simulationOwnershipData.RevokeOwnerOfId(packet.Id))
            {
                ushort serverId = ushort.MaxValue;
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(packet.Id, serverId, SimulationLockType.TRANSIENT);
                playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
            }
            
            entityManager.PickUpEntity(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
