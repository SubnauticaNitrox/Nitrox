using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class ItemContainers
    {
        private PacketSender packetSender;

        public ItemContainers(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void AddItem(Pickupable pickupable, GameObject owner)
        {
            String ownerGuid = GuidHelper.GetGuid(owner);
            Vector3 ownerPos = owner.transform.position;
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);
            
            ItemContainerAdd add = new ItemContainerAdd(packetSender.PlayerId, ownerGuid, bytes, ApiHelper.Vector3(ownerPos));
            packetSender.Send(add);

            Console.WriteLine(add);
            Console.WriteLine(DebugUtils.ByteArrayToHexString(bytes));
        }

        public void RemoveItem(Pickupable pickupable, GameObject owner)
        {
            String itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            String ownerGuid = GuidHelper.GetGuid(owner);
            Vector3 ownerPos = owner.transform.position;

            ItemContainerRemove remove = new ItemContainerRemove(packetSender.PlayerId, ownerGuid, itemGuid, ApiHelper.Vector3(ownerPos));
            packetSender.Send(remove);
        }
    }
}
