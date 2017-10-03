using NitroxClient.Communication;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Item
    {
        private PacketSender packetSender;

        public Item(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void UpdatePosition(String guid, Vector3 location, Quaternion rotation)
        {
            ItemPosition itemPosition = new ItemPosition(packetSender.PlayerId, guid, location, rotation);
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
            PickupItem pickupItem = new PickupItem(packetSender.PlayerId, itemPosition, guid, techType);
            packetSender.Send(pickupItem);
        }

        public void Dropped(GameObject gameObject, TechType techType, Vector3 dropPosition)
        {
            Optional<String> waterpark = GetCurrentWaterParkGuid();            
            String guid = GuidHelper.GetGuid(gameObject);
            byte[] bytes = SerializationHelper.GetBytes(gameObject);

            SyncedMultiplayerObject.ApplyTo(gameObject);

            Log.Debug("Dropping item with guid: " + guid);

            DroppedItem droppedItem = new DroppedItem(packetSender.PlayerId, guid, waterpark, techType, dropPosition, bytes);
            packetSender.Send(droppedItem);
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
