using NitroxClient.Communication.Packets.Processors.Base;
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
            float sphereSize = 0.01f;
            Collider[] colliders = Physics.OverlapSphere(ApiHelper.Vector3(pickup.ItemPosition), sphereSize);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.name.Equals(pickup.GameObjectName))
                {
                    Console.WriteLine("Destroying " + collider.gameObject.name);
                    UnityEngine.Object.Destroy(collider.gameObject);
                }
                else if (collider.gameObject.transform.parent.name.Equals(pickup.GameObjectName))
                {
                    Console.WriteLine("Destroying " + collider.gameObject.transform.parent.name);
                    UnityEngine.Object.Destroy(collider.gameObject.transform.parent.gameObject);
                }
            }
        }
    }
}
