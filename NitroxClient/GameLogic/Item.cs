using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.IO;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Item
    {
        private PacketSender packetSender;
        private ProtobufSerializer serializer;

        public Item(PacketSender packetSender)
        {
            this.packetSender = packetSender;
            this.serializer = new ProtobufSerializer();
        }

        public void UpdatePosition(String guid, Vector3 location, Quaternion rotation)
        {
            ItemPosition itemPosition = new ItemPosition(packetSender.PlayerId, guid, ApiHelper.Vector3(location), ApiHelper.Quaternion(rotation));
            packetSender.Send(itemPosition);
        }

        public void PickedUp(GameObject gameObject, String techType)
        {
            String guid = GuidHelper.GetGuid(gameObject);
            Vector3 itemPosition = gameObject.transform.position;

            PickedUp(itemPosition, guid, techType);
        }

        public void PickedUp(Vector3 itemPosition, String guid, String techType)
        {
            PickupItem pickupItem = new PickupItem(packetSender.PlayerId, ApiHelper.Vector3(itemPosition), guid, techType);
            packetSender.Send(pickupItem);
        }

        public void Dropped(GameObject gameObject, TechType techType, Vector3 dropPosition)
        {
            Optional<String> waterpark = GetCurrentWaterParkGuid();
            
            String guid = GuidHelper.GetGuid(gameObject);

            byte[] bytes;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.SerializeObjectTree(memoryStream, gameObject);
                bytes = memoryStream.ToArray();
            }

            SyncedMultiplayerObject.ApplyTo(gameObject);

            Console.WriteLine("Dropping item with guid: " + guid);

            DroppedItem droppedItem = new DroppedItem(packetSender.PlayerId, guid, waterpark, ApiHelper.TechType(techType), ApiHelper.Vector3(dropPosition), bytes);
            packetSender.Send(droppedItem);
            Console.WriteLine(droppedItem);
        }

        private Optional<String> GetCurrentWaterParkGuid()
        {
            Player player = Utils.GetLocalPlayer().GetComponent<Player>();

            if (player != null)
            {
                WaterPark currentWaterPark = player.currentWaterPark;

                if (currentWaterPark != null)
                {
                    String waterParkGuid = GuidHelper.GetGuid(currentWaterPark.gameObject);
                    return Optional<String>.Of(waterParkGuid);
                }
            }

            return Optional<String>.Empty();
        }
    }
}
