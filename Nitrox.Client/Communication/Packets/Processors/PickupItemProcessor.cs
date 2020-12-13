using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
