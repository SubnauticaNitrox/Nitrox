using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SimulationOwnershipChangeProcessor(SimulationOwnership simulationOwnershipManager) : IClientPacketProcessor<SimulationOwnershipChange>
{
    private readonly SimulationOwnership simulationOwnershipManager = simulationOwnershipManager;

    public Task Process(ClientProcessorContext context, SimulationOwnershipChange simulationOwnershipChange)
    {
        foreach (SimulatedEntity simulatedEntity in simulationOwnershipChange.Entities)
        {
            simulationOwnershipManager.TreatSimulatedEntity(simulatedEntity);
        }
        return Task.CompletedTask;
    }
}
