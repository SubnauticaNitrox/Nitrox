using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PickupItemProcessor : ClientPacketProcessor<PickupItem>
    {
        private readonly Entities entities;

        public PickupItemProcessor(Entities entities)
        {
            this.entities = entities;
        }

        public override void Process(PickupItem pickup)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(pickup.Id);
            if (opGameObject.HasValue)
            {
                Object.Destroy(opGameObject.Value);
                entities.RemoveEntity(pickup.Id);
            }
        }
    }
}
