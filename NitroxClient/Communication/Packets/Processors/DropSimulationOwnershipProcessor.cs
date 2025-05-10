using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DropSimulationOwnershipProcessor : IClientPacketProcessor<DropSimulationOwnership>
{
    private readonly SimulationOwnership simulationOwnershipManager;

    public DropSimulationOwnershipProcessor(SimulationOwnership simulationOwnershipManager)
    {
        this.simulationOwnershipManager = simulationOwnershipManager;
    }

    public Task Process(IPacketProcessContext context, DropSimulationOwnership packet)
    {
        simulationOwnershipManager.DropSimulationFrom(packet.EntityId);
        return Task.CompletedTask;
    }
}
