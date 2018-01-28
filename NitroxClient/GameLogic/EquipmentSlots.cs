using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class EquipmentSlots
    {
        private readonly IPacketSender packetSender;

        public EquipmentSlots(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Equip(Pickupable pickupable, GameObject owner, string slot)
        {
            string ownerGuid = GuidHelper.GetGuid(owner);
            Vector3 ownerPos = owner.transform.position;
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            EquipmentAddItem equip = new EquipmentAddItem(ownerGuid, slot, bytes, ownerPos);
            packetSender.send(equip);
        }

        public void Unequip(Pickupable pickupable, GameObject owner, string slot)
        {
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            string ownerGuid = GuidHelper.GetGuid(owner);
            Vector3 ownerPos = owner.transform.position;

            EquipmentRemoveItem removeEquipment = new EquipmentRemoveItem(ownerGuid, slot, itemGuid, ownerPos);
            packetSender.send(removeEquipment);
        }
    }
}
