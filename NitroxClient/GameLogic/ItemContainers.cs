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
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            ItemContainerAdd add = new ItemContainerAdd(ownerGuid, bytes);
            packetSender.Send(add);
        }

        public void RemoveItem(Pickupable pickupable, GameObject owner)
        {
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            string ownerGuid = GuidHelper.GetGuid(owner);

            ItemContainerRemove remove = new ItemContainerRemove(ownerGuid, itemGuid);
            packetSender.Send(remove);
        }
    }
}
