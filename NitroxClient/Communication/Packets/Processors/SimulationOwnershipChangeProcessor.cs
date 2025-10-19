using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SimulationOwnershipChangeProcessor : ClientPacketProcessor<SimulationOwnershipChange>
{
    private readonly SimulationOwnership simulationOwnershipManager;

    public SimulationOwnershipChangeProcessor(SimulationOwnership simulationOwnershipManager)
    {
        this.simulationOwnershipManager = simulationOwnershipManager;
    }

    public override void Process(SimulationOwnershipChange simulationOwnershipChange)
    {
        foreach (SimulatedEntity simulatedEntity in simulationOwnershipChange.Entities)
        {
            simulationOwnershipManager.TreatSimulatedEntity(simulatedEntity);
        }
    }
}

