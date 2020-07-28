using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class SimulationOwnershipRequestProcessor : AuthenticatedPacketProcessor<SimulationOwnershipRequest>
    {
        private readonly PlayerManager playerManager;
        private readonly SimulationOwnershipData simulationOwnershipData;

        public SimulationOwnershipRequestProcessor(PlayerManager playerManager, SimulationOwnershipData simulationOwnershipData)
        {
            this.playerManager = playerManager;
            this.simulationOwnershipData = simulationOwnershipData;
        }

        public override void Process(SimulationOwnershipRequest ownershipRequest, Player player)
        {
            bool aquiredLock = simulationOwnershipData.TryToAcquire(ownershipRequest.Id, player, ownershipRequest.LockType);

            if (aquiredLock)
            {
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(ownershipRequest.Id, player.Id, ownershipRequest.LockType);
                playerManager.SendPacketToOtherPlayers(simulationOwnershipChange, player);
            }

            SimulationOwnershipResponse responseToPlayer = new SimulationOwnershipResponse(ownershipRequest.Id, aquiredLock, ownershipRequest.LockType);
            player.SendPacket(responseToPlayer);
        }
    }
}
