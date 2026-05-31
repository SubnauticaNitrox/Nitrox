using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class DropSimulationOwnershipProcessor(SimulationOwnership simulationOwnershipManager) : IClientPacketProcessor<DropSimulationOwnership>
{
    private readonly SimulationOwnership simulationOwnershipManager = simulationOwnershipManager;

    public Task Process(ClientProcessorContext context, DropSimulationOwnership packet)
    {
        simulationOwnershipManager.DropSimulationFrom(packet.EntityId);
        return Task.CompletedTask;
    }
}
