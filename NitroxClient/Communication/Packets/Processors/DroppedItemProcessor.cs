using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.ItemDropActions;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class DroppedItemProcessor : ClientPacketProcessor<DroppedItem>
    {
        public override void Process(DroppedItem drop)
        {
            GameObject gameObject = SerializationHelper.GetGameObject(drop.Bytes);
            gameObject.transform.position = drop.ItemPosition;

            Pickupable pickupable = gameObject.GetComponent<Pickupable>();

            if (drop.WaterParkGuid.IsPresent())
            {
                AssignToWaterPark(drop.WaterParkGuid.Get(), pickupable);
            }

            EnableRigidBody(gameObject);
            ExecuteDropItemAction(drop.TechType, gameObject);
            RemoveExistingSyncScripts(gameObject);
        }

        private void EnableRigidBody(GameObject gameObject)
        {
            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();

            if (rigidBody != null)
            {
                rigidBody.isKinematic = false;
            }
            else
            {
                Console.WriteLine("No rigid body!");
            }
        }

        private void AssignToWaterPark(String waterParkGuid, Pickupable pickupable)
        {
            Optional<GameObject> opWaterPark = GuidHelper.GetObjectFrom(waterParkGuid);

            if (opWaterPark.IsPresent())
            {
                WaterPark waterPark = opWaterPark.Get().GetComponent<WaterPark>();

                if (waterPark != null)
                {
                    waterPark.AddItem(pickupable);
                }
                else
                {
                    Console.WriteLine("Could not find water park component on that game object");
                }
            }
            else
            {
                Console.WriteLine("Could not locate water park with guid: " + waterParkGuid);
            }
        }

        private void ExecuteDropItemAction(TechType techType, GameObject gameObject)
        {
            Console.WriteLine("Performing drop action for tech type: " + techType);

            ItemDropAction itemDropAction = ItemDropAction.FromTechType(techType);
            itemDropAction.ProcessDroppedItem(gameObject);
        }

        private void RemoveExistingSyncScripts(GameObject gameObject)
        {
            SyncedMultiplayerObject smo = gameObject.GetComponent<SyncedMultiplayerObject>();

            if (smo != null)
            {
                UnityEngine.Object.Destroy(smo);
            }
        }
    }
}
