using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;
using static Nitrox.Model.Packets.EntityTransformUpdates;

namespace Nitrox.Client.Communication.Packets.Processors
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

                remotelyControlled.UpdateOrientation(entity.Position.ToUnity(), entity.Rotation.ToUnity());
            }
        }
    }
}
