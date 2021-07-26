using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
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
                
                GameObject gameObject = opGameObject.Value;
                float distance = Vector3.Distance(gameObject.transform.position, entity.Position);
                SwimBehaviour swimBehaviour = gameObject.GetComponent<SwimBehaviour>();

                if (distance > 5 || swimBehaviour == null)
                {
                    gameObject.transform.position = entity.Position;
                    gameObject.transform.rotation = entity.Rotation;
                }
                else
                {
                    swimBehaviour.SwimTo(entity.Position, 3f);
                }
            }
        }
    }
}
