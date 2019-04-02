using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
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
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(pickup.Guid);

            if (opGameObject.IsPresent())
            {
                UnityEngine.Object.Destroy(opGameObject.Get());
               entities.RemoveEntity(pickup.Guid);
            }
        }
    }
}
