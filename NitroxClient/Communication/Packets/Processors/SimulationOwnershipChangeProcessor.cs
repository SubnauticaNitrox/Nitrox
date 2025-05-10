using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SimulationOwnershipChangeProcessor : IClientPacketProcessor<SimulationOwnershipChange>
{
    private readonly SimulationOwnership simulationOwnershipManager;

    public SimulationOwnershipChangeProcessor(SimulationOwnership simulationOwnershipManager)
    {
        this.simulationOwnershipManager = simulationOwnershipManager;
    }

    public Task Process(IPacketProcessContext context, SimulationOwnershipChange simulationOwnershipChange)
    {
        foreach (SimulatedEntity simulatedEntity in simulationOwnershipChange.Entities)
        {
            simulationOwnershipManager.TreatSimulatedEntity(simulatedEntity);
        }
        return Task.CompletedTask;
    }
}
