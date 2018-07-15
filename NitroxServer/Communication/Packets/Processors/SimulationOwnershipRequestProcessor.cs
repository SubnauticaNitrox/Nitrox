using NitroxModel.Logger;
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
            Log.Debug(ownershipRequest);

            if (simulationOwnershipData.TryToAcquire(ownershipRequest.Guid, player))
            {
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(ownershipRequest.Guid, player.Id);
                playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
                Log.Debug("Sending: " + simulationOwnershipChange);
            }
        }
    }
}
