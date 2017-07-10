using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PickupItemProcessor : GenericPacketProcessor<PickupItem>
    {
        public override void Process(PickupItem pickup)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(pickup.Guid);

            if(opGameObject.IsPresent())
            {
                UnityEngine.Object.Destroy(opGameObject.Get());
            }
        }
    }
}
