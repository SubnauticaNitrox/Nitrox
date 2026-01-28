using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SimulationOwnershipResponseProcessor(IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnershipManager) : IClientPacketProcessor<SimulationOwnershipResponse>
{
    private readonly IMultiplayerSession multiplayerSession = multiplayerSession;
    private readonly SimulationOwnership simulationOwnershipManager = simulationOwnershipManager;

    public Task Process(ClientProcessorContext context, SimulationOwnershipResponse response)
    {
        /*
         * For now, we expect the simulation lock callback to setup entity broadcasting as
         * most items that are requesting an exclusive lock have custom broadcast code, ex:
         * vehicles like the cyclops.  However, we may one day want to add a watcher here
         * to ensure broadcast one day, ex:
         *
         * EntityPositionBroadcaster.WatchEntity(simulatedEntity.Id, gameObject.Value);
         *
         */
        simulationOwnershipManager.ReceivedSimulationLockResponse(response.Id, response.LockAcquired, response.LockType);

        if (response.LockAcquired)
        {
            RemoveRemoteController(response.Id);
        }
        return Task.CompletedTask;
    }

    private void RemoveRemoteController(NitroxId id)
    {
        Optional<GameObject> gameObject = NitroxEntity.GetObjectFrom(id);

        if (gameObject.HasValue)
        {
            RemotelyControlled remotelyControlled = gameObject.Value.GetComponent<RemotelyControlled>();
            Object.Destroy(remotelyControlled);
        }
    }
}
