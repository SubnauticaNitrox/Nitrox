using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class SimulationOwnershipRequestProcessor : AuthenticatedPacketProcessor<SimulationOwnershipRequest>
    {
        private TcpServer tcpServer;
        private SimulationOwnership simulationOwnership;

        public SimulationOwnershipRequestProcessor(TcpServer tcpServer, SimulationOwnership simulationOwnership)
        {
            this.tcpServer = tcpServer;
            this.simulationOwnership = simulationOwnership;
        }
        
        public override void Process(SimulationOwnershipRequest ownershipRequest, Player player)
        {
            Log.Debug(ownershipRequest);

            if(simulationOwnership.TryToAquireOwnership(ownershipRequest.Guid, player))
            {
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(ownershipRequest.Guid, player.Id);
                tcpServer.SendPacketToAllPlayers(simulationOwnershipChange);
                Log.Debug("Sending: " + simulationOwnershipChange);
            }
        }
    }
}
