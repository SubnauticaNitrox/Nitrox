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
    public class ConstructionAmountChangedProcessor : GenericPacketProcessor<ConstructionAmountChanged>
    {
        public override void Process(ConstructionAmountChanged amountChanged)
        {
            Console.WriteLine("Processing ConstructionAmountChanged " + amountChanged.Guid + " " + amountChanged.PlayerId + " " + amountChanged.ConstructionAmount);

            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(amountChanged.Guid);

            if(opGameObject.IsPresent())
            {
                GameObject constructing = opGameObject.Get();
                Constructable constructable = constructing.GetComponent<Constructable>();
                constructable.constructedAmount = amountChanged.ConstructionAmount;
                constructable.Construct();
            }
        }
    }
}
