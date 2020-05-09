using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Smoothing;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxModel.Packets.EntityTransformUpdates;

namespace NitroxClient.Communication.Packets.Processors
{
    class EntityTransformUpdatesProcessor : ClientPacketProcessor<EntityTransformUpdates>
    {
        public override void Process(EntityTransformUpdates packet)
        {
            foreach (EntityTransformUpdate entity in packet.Updates)
            {
                Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(entity.Id);

                if (!opGameObject.HasValue)
                {
                    continue;
                }

                RemotelyControlled remotelyControlled = opGameObject.Value.GetComponent<RemotelyControlled>();

                if (!remotelyControlled)
                {
                    remotelyControlled = opGameObject.Value.AddComponent<RemotelyControlled>();
                }

                remotelyControlled.UpdateOrientation(entity.Position, entity.Rotation);
            }
        }
    }
}
