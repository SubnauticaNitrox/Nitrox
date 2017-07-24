using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.ItemDropActions;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.IO;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class DroppedItemProcessor : ClientPacketProcessor<DroppedItem>
    {
        private ProtobufSerializer serializer = new ProtobufSerializer();

        public override void Process(DroppedItem drop)
        {
            GameObject gameObject;

            using (MemoryStream memoryStream = new MemoryStream(drop.Bytes))
            {
                gameObject = serializer.DeserializeObjectTree(memoryStream, 0);
            }

            gameObject.transform.position = ApiHelper.Vector3(drop.ItemPosition);
            
            Pickupable pickupable = gameObject.GetComponent<Pickupable>();

            if(drop.WaterParkGuid.IsPresent())
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

        private void ExecuteDropItemAction(String techTypeString, GameObject gameObject)
        {
            Optional<TechType> opTechType = ApiHelper.TechType(techTypeString);

            if (opTechType.IsEmpty())
            {
                Console.WriteLine("Attempted to drop unknown tech type: " + techTypeString);
                return;
            }

            TechType techType = opTechType.Get();

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
