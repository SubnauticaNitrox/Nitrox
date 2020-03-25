using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
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

        public void UpdatePosition(NitroxId id, Vector3 location, Quaternion rotation)
        {
            ItemPosition itemPosition = new ItemPosition(id, location, rotation);
            packetSender.Send(itemPosition);
        }

        public void PickedUp(GameObject gameObject, TechType techType)
        {
            NitroxId id = NitroxEntity.GetId(gameObject);
            Vector3 itemPosition = gameObject.transform.position;

            PickedUp(itemPosition, id, techType);
        }

        public void PickedUp(Vector3 itemPosition, NitroxId id, TechType techType)
        {
            Log.Info("PickedUp " + id + " " + techType);
            PickupItem pickupItem = new PickupItem(itemPosition, id, techType.Model());
            packetSender.Send(pickupItem);
        }

        public void Dropped(GameObject gameObject, TechType techType, Vector3 dropPosition)
        {
            Optional<NitroxId> waterparkId = GetCurrentWaterParkId();
            NitroxId id = NitroxEntity.GetId(gameObject);
            byte[] bytes = SerializationHelper.GetBytes(gameObject);
            
            Log.Debug("Dropping item with id: " + id);

            DroppedItem droppedItem = new DroppedItem(id, waterparkId, techType.Model(), dropPosition, gameObject.transform.rotation, bytes);
            packetSender.Send(droppedItem);
        }

        private Optional<NitroxId> GetCurrentWaterParkId()
        {
            Player player = Utils.GetLocalPlayer().GetComponent<Player>();

            if (player != null)
            {
                WaterPark currentWaterPark = player.currentWaterPark;

                if (currentWaterPark != null)
                {
                    NitroxId waterParkId = NitroxEntity.GetId(currentWaterPark.gameObject);
                    return Optional.Of(waterParkId);
                }
            }

            return Optional.Empty;
        }
    }
}
