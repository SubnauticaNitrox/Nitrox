using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxModel.Packets.EntityTransformUpdates;

namespace NitroxClient.Communication.Packets.Processors;

public class EntityTransformUpdatesProcessor : ClientPacketProcessor<EntityTransformUpdates>
{
    private readonly SimulationOwnership simulationOwnership;

    public EntityTransformUpdatesProcessor(SimulationOwnership simulationOwnership)
    {
        this.simulationOwnership = simulationOwnership;
    }

    public override void Process(EntityTransformUpdates packet)
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

            if (update is SplineTransformUpdate splineUpdate)
            {
                remotelyControlled.UpdateKnownSplineUser(splineUpdate.Position.ToUnity(), splineUpdate.Rotation.ToUnity(), splineUpdate.DestinationPosition.ToUnity(), splineUpdate.DestinationDirection.ToUnity(), splineUpdate.Velocity);
            }
            else
            {
                remotelyControlled.UpdateOrientation(update.Position.ToUnity(), update.Rotation.ToUnity());
            }
        }
    }
}
