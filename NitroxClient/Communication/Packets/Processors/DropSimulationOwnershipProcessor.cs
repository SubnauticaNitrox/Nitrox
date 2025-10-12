using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DropSimulationOwnershipProcessor : ClientPacketProcessor<DropSimulationOwnership>
{
    private readonly SimulationOwnership simulationOwnershipManager;

    public DropSimulationOwnershipProcessor(SimulationOwnership simulationOwnershipManager)
    {
        this.simulationOwnershipManager = simulationOwnershipManager;
    }

    public override void Process(DropSimulationOwnership packet)
    {
        simulationOwnershipManager.DropSimulationFrom(packet.EntityId);
    }
}
