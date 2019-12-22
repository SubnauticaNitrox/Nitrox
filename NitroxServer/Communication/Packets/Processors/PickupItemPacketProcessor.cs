using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
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
                NitroxId serverId = NitroxId.Empty;
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(packet.Id, serverId, NitroxModel.DataStructures.SimulationLockType.TRANSIENT);
                playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
            }
            
            entityManager.PickUpEntity(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
