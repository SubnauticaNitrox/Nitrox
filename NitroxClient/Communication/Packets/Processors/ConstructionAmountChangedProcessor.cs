using NitroxClient.Communication.Packets.Processors.Base;
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
            Console.WriteLine("Processing ConstructionAmountChanged " + amountChanged.ItemPosition + " " + amountChanged.PlayerId + " " + amountChanged.ConstructionAmount);

            Constructable[] constructables = GameObject.FindObjectsOfType<Constructable>();

            Console.WriteLine("constructables " + constructables.Length);

            foreach (Constructable constructable in constructables)
            {
                if (constructable.transform.position == ApiHelper.Vector3(amountChanged.ItemPosition))
                {
                    Console.WriteLine("Found constructable!");
                    constructable.constructedAmount = amountChanged.ConstructionAmount;
                    constructable.Construct();
                }
            }
        }
    }
}
