using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class ItemContainers
    {
        private readonly IPacketSender packetSender;

        public ItemContainers(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void AddItem(Pickupable pickupable, GameObject owner)
        {
            string ownerGuid = GuidHelper.GetGuid(owner);
            Vector3 ownerPos = owner.transform.position;
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            ItemContainerAdd add = new ItemContainerAdd(ownerGuid, bytes, ownerPos);
            packetSender.Send(add);
        }

        public void RemoveItem(Pickupable pickupable, GameObject owner)
        {
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            string ownerGuid = GuidHelper.GetGuid(owner);
            Vector3 ownerPos = owner.transform.position;

            ItemContainerRemove remove = new ItemContainerRemove(ownerGuid, itemGuid, ownerPos);
            packetSender.Send(remove);
        }
    }
}
