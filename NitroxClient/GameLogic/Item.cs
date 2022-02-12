using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
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
            ItemPosition itemPosition = new ItemPosition(id, location.ToDto(), rotation.ToDto());
            packetSender.Send(itemPosition);
        }

        public void PickedUp(GameObject gameObject, TechType techType)
        {
            // We want to remove any remote tracking immediately on pickup as it can cause weird behavior like holding a ghost item still in the world.
            RemoveAnyRemoteControl(gameObject);

            NitroxId id = NitroxEntity.GetId(gameObject);
            Vector3 itemPosition = gameObject.transform.position;

            Log.Info("PickedUp " + id + " " + techType);

            PickupItem pickupItem = new PickupItem(itemPosition.ToDto(), id, techType.ToDto());
            packetSender.Send(pickupItem);
        }

        public void Dropped(GameObject gameObject, TechType techType, Vector3 dropPosition)
        {
            // there is a theoretical possibility of a stray remote tracking packet that re-adds the monobehavior, this is purely a safety call.
            RemoveAnyRemoteControl(gameObject);

            Optional<NitroxId> waterparkId = GetCurrentWaterParkId();
            NitroxId id = NitroxEntity.GetId(gameObject);
            byte[] bytes = SerializationHelper.GetBytesWithoutParent(gameObject);

            Log.Debug("Dropping item with id: " + id);

            DroppedItem droppedItem = new DroppedItem(id, waterparkId, techType.ToDto(), dropPosition.ToDto(), gameObject.transform.rotation.ToDto(), bytes);
            packetSender.Send(droppedItem);
        }

        private void RemoveAnyRemoteControl(GameObject gameObject)
        {
            // Some items might be remotely simulated if they were dropped by other players.  We'll want to remove
            // any remote tracking when we actively handle the item. 
            RemotelyControlled remotelyControlled = gameObject.GetComponent<RemotelyControlled>();
            Object.Destroy(remotelyControlled);
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
