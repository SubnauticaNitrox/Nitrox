using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxModel.Packets.EntityTransformUpdates;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.Packets.Processors;

public class EntityTransformUpdatesProcessor : ClientPacketProcessor<EntityTransformUpdates>
{
    public override void Process(EntityTransformUpdates packet)
    {
        // Get position updates and enact the changes on client-side
        foreach (EntityTransformUpdate update in packet.Updates)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(update.Id);

            if (!opGameObject.HasValue)
            {
                continue;
            } else
            {
                DisplayStatusCode(StatusCode.invalidPacket);
            }

            RemotelyControlled remotelyControlled = opGameObject.Value.GetComponent<RemotelyControlled>();

            if (!remotelyControlled)
            {
                remotelyControlled = opGameObject.Value.AddComponent<RemotelyControlled>();
            }

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
