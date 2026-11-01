using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SimulationOwnershipChangeProcessor(SimulationOwnership simulationOwnershipManager, PlayerManager playerManager) : IClientPacketProcessor<SimulationOwnershipChange>
{
    private readonly SimulationOwnership simulationOwnershipManager = simulationOwnershipManager;
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, SimulationOwnershipChange simulationOwnershipChange)
    {
        foreach (SimulatedEntity simulatedEntity in simulationOwnershipChange.Entities)
        {
            simulationOwnershipManager.TreatSimulatedEntity(simulatedEntity);
            ReleaseStaleCinematicLock(simulatedEntity);
        }
        return Task.CompletedTask;
    }

    // Backstop for a lost cinematic-end packet
    private void ReleaseStaleCinematicLock(SimulatedEntity simulatedEntity)
    {
        if (simulatedEntity.LockType == SimulationLockType.EXCLUSIVE)
        {
            return;
        }

        foreach (RemotePlayer player in playerManager.GetAll())
        {
            if (player.InCinematicEntityId == simulatedEntity.Id && player.SessionId != simulatedEntity.SessionId)
            {
                player.ClearInCinematic();
            }
        }
    }
}
