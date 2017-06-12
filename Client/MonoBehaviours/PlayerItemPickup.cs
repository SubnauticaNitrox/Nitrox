using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerItemPickup : MonoBehaviour
    {
        public void Update()
        {
            Queue<PickupItem> pickups = Multiplayer.client.getPickedUpItems();

            while (pickups.Count > 0)
            {
                PickupItem pickup = pickups.Dequeue();
                float sphereSize = 0.01f;
                Collider[] colliders = Physics.OverlapSphere(ApiHelper.Vector3(pickup.ItemPosition), sphereSize);

                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.name.Equals(pickup.GameObjectName))
                    {
                        Console.WriteLine("Destroying " + collider.gameObject.name);
                        Destroy(collider.gameObject);
                    }
                    else if (collider.gameObject.transform.parent.name.Equals(pickup.GameObjectName))
                    {
                        Console.WriteLine("Destroying " + collider.gameObject.transform.parent.name);
                        Destroy(collider.gameObject.transform.parent.gameObject);
                    }
                }
            }
        }
    }
}
