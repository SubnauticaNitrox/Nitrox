using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Item
    {
        private readonly IPacketSender packetSender;

        public Item(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void UpdatePosition(string guid, Vector3 location, Quaternion rotation)
        {
            ItemPosition itemPosition = new ItemPosition(guid, location, rotation);
            packetSender.Send(itemPosition);
        }

        public void PickedUp(GameObject gameObject, string techType)
        {
            string guid = GuidHelper.GetGuid(gameObject);
            Vector3 itemPosition = gameObject.transform.position;

            PickedUp(itemPosition, guid, techType);
        }

        public void PickedUp(Vector3 itemPosition, string guid, string techType)
        {
            PickupItem pickupItem = new PickupItem(itemPosition, guid, techType);
            packetSender.Send(pickupItem);
        }

        public void Dropped(GameObject gameObject, TechType techType, Vector3 dropPosition)
        {
            Optional<string> waterpark = GetCurrentWaterParkGuid();
            string guid = GuidHelper.GetGuid(gameObject);
            byte[] bytes = SerializationHelper.GetBytes(gameObject);

            SyncedMultiplayerObject.ApplyTo(gameObject);

            Log.Debug("Dropping item with guid: " + guid);

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
            DroppedItem droppedItem = new DroppedItem(guid, waterpark, techType, dropPosition, bytes);
            packetSender.Send(droppedItem);

=======
            if (techType == TechType.Beacon)
=======
            if (techType == TechType.Beacon | techType == TechType.Pipe)
>>>>>>> c7606c2... Changes Requested
            {
                DroppedItemPacket droppedItem = new DroppedItemPacket(guid, waterpark, techType, dropPosition, bytes);
                packetSender.Send(droppedItem);
            }
            else
            {
                DroppedItem droppedItem = new DroppedItem(guid, waterpark, techType, dropPosition, bytes);
                packetSender.Send(droppedItem);
            }

            
>>>>>>> f39663e... Item Beacon Fix Sync
=======
            DroppedItem droppedItem = new DroppedItem(guid, waterpark, techType, dropPosition, bytes);
            packetSender.Send(droppedItem);

>>>>>>> 6ac5fe5... Requested Changes
        }

        private Optional<string> GetCurrentWaterParkGuid()
        {
            Player player = Utils.GetLocalPlayer().GetComponent<Player>();

            if (player != null)
            {
                WaterPark currentWaterPark = player.currentWaterPark;

                if (currentWaterPark != null)
                {
                    string waterParkGuid = GuidHelper.GetGuid(currentWaterPark.gameObject);
                    return Optional<string>.Of(waterParkGuid);
                }
            }

            return Optional<string>.Empty();
        }
    }
}
