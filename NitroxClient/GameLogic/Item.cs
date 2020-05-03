using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using DTO = NitroxModel.DataStructures;

namespace NitroxClient.GameLogic
{
    public class Item
    {
        private readonly IPacketSender packetSender;

        public Item(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void UpdatePosition(DTO.NitroxId id, Vector3 location, Quaternion rotation)
        {
            ItemPosition itemPosition = new ItemPosition(id, location.ToDto(), rotation.ToDto());
            packetSender.Send(itemPosition);
        }

        public void PickedUp(GameObject gameObject, TechType techType)
        {
            DTO.NitroxId id = NitroxEntity.GetId(gameObject);
            Vector3 itemPosition = gameObject.transform.position;

            PickedUp(itemPosition, id, techType);
        }

        public void PickedUp(Vector3 itemPosition, DTO.NitroxId id, TechType techType)
        {
            Log.Info("PickedUp " + id + " " + techType);
            PickupItem pickupItem = new PickupItem(itemPosition.ToDto(), id, techType.ToDto());
            packetSender.Send(pickupItem);
        }

        public void Dropped(GameObject gameObject, TechType techType, Vector3 dropPosition)
        {
            Optional<DTO.NitroxId> waterparkId = GetCurrentWaterParkId();
            DTO.NitroxId id = NitroxEntity.GetId(gameObject);
            byte[] bytes = SerializationHelper.GetBytes(gameObject);

            Log.Debug("Dropping item with id: " + id);

            DroppedItem droppedItem = new DroppedItem(id, waterparkId, techType.ToDto(), dropPosition.ToDto(), gameObject.transform.rotation.ToDto(), bytes);
            packetSender.Send(droppedItem);
        }

        private Optional<DTO.NitroxId> GetCurrentWaterParkId()
        {
            Player player = Utils.GetLocalPlayer().GetComponent<Player>();

            if (player != null)
            {
                WaterPark currentWaterPark = player.currentWaterPark;

                if (currentWaterPark != null)
                {
                    DTO.NitroxId waterParkId = NitroxEntity.GetId(currentWaterPark.gameObject);
                    return Optional.Of(waterParkId);
                }
            }

            return Optional.Empty;
        }
    }
}
