using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using static Nitrox.Model.Subnautica.Packets.EntityTransformUpdates;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class EntityTransformUpdatesProcessor(SimulationOwnership simulationOwnership) : IClientPacketProcessor<EntityTransformUpdates>
{
    private readonly SimulationOwnership simulationOwnership = simulationOwnership;

    public Task Process(ClientProcessorContext context, EntityTransformUpdates packet)
    {
        foreach (EntityTransformUpdate update in packet.Updates)
        {
            // We will cancel any position update attempt at one of our locked entities
            if (!NitroxEntity.TryGetObjectFrom(update.Id, out GameObject gameObject) ||
                simulationOwnership.HasAnyLockType(update.Id))
            {
                continue;
            }

            // Spline updates are only meant for living creatures, but the simulating player can still send some before learning
            // that we killed it or that it turned into something else with the same id (e.g. a cooked fish)
            if (update is SplineTransformUpdate && !CanFollowSpline(gameObject))
            {
                continue;
            }

            RemotelyControlled remotelyControlled = RemotelyControlled.Ensure(gameObject);

            Vector3 position = update.Position.ToUnity();
            Quaternion rotation = update.Rotation.ToUnity();

            if (update is SplineTransformUpdate splineUpdate)
            {
                remotelyControlled.UpdateKnownSplineUser(position, rotation, splineUpdate.DestinationPosition.ToUnity(), splineUpdate.DestinationDirection.ToUnity(), splineUpdate.Velocity);
            }
            else
            {
                remotelyControlled.UpdateOrientation(position, rotation);
            }
        }
        return Task.CompletedTask;
    }

    private static bool CanFollowSpline(GameObject gameObject)
    {
        if (!gameObject.GetComponent<SwimBehaviour>() && !gameObject.GetComponent<WalkBehaviour>())
        {
            return false;
        }

        return !gameObject.TryGetComponent(out CreatureDeath creatureDeath) || !creatureDeath.liveMixin || creatureDeath.liveMixin.IsAlive();
    }
}
