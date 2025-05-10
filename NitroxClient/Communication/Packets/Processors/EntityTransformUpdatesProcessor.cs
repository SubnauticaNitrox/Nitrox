using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;
using UnityEngine;
using static NitroxModel.Networking.Packets.EntityTransformUpdates;

namespace NitroxClient.Communication.Packets.Processors;

public class EntityTransformUpdatesProcessor : IClientPacketProcessor<EntityTransformUpdates>
{
    private readonly SimulationOwnership simulationOwnership;

    public EntityTransformUpdatesProcessor(SimulationOwnership simulationOwnership)
    {
        this.simulationOwnership = simulationOwnership;
    }

    public Task Process(IPacketProcessContext context, EntityTransformUpdates packet)
    {
        foreach (EntityTransformUpdate update in packet.Updates)
        {
            // We will cancel any position update attempt at one of our locked entities
            if (!NitroxEntity.TryGetObjectFrom(update.Id, out GameObject gameObject) ||
                simulationOwnership.HasAnyLockType(update.Id))
            {
                continue;
            }

            RemotelyControlled remotelyControlled = gameObject.EnsureComponent<RemotelyControlled>();

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
}
