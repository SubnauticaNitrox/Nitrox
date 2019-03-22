using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
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

        public void PickedUp(GameObject gameObject, TechType techType)
        {
            string guid = GuidHelper.GetGuid(gameObject);
            Vector3 itemPosition = gameObject.transform.position;

            PickedUp(itemPosition, guid, techType);
        }

        public void PickedUp(Vector3 itemPosition, string guid, TechType techType)
        {
            Log.Info("PickedUp " + guid + " " + techType);
            PickupItem pickupItem = new PickupItem(itemPosition, guid, techType.Model());
            packetSender.Send(pickupItem);
        }

        public void Dropped(GameObject gameObject, TechType techType, Vector3 dropPosition)
        {
            Optional<string> waterpark = GetCurrentWaterParkGuid();
            string guid = GuidHelper.GetGuid(gameObject);
            byte[] bytes = SerializationHelper.GetBytes(gameObject);
            
            Log.Debug("Dropping item with guid: " + guid);

            DroppedItem droppedItem = new DroppedItem(guid, waterpark, techType.Model(), dropPosition, gameObject.transform.rotation, bytes);
            packetSender.Send(droppedItem);
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
