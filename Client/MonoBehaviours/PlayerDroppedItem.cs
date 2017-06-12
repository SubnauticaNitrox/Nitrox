using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerDroppedItem : MonoBehaviour
    {
        public void Update()
        {
            Queue<DroppedItem> drops = Multiplayer.client.getDroppedItems();

            while (drops.Count > 0)
            {
                DroppedItem drop = drops.Dequeue();

                TechType techType;

                UWE.Utils.TryParseEnum<TechType>(drop.TechType, out techType);

                GameObject techPrefab = TechTree.main.GetGamePrefab(techType);

                if (techPrefab != null)
                {
                    GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(techPrefab, ApiHelper.Vector3(drop.ItemPosition), Quaternion.FromToRotation(Vector3.up, Vector3.up));
                    gameObject.SetActive(true);
                    CrafterLogic.NotifyCraftEnd(gameObject, techType);
                    gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);

                    Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
                    rigidBody.isKinematic = false;
                    rigidBody.AddForce(ApiHelper.Vector3(drop.PushVelocity), ForceMode.VelocityChange);
                }
            }
        }
    }
}
