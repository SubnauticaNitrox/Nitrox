using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class EquipmentSlots
    {
        private readonly PacketSender packetSender;

        public EquipmentSlots(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Equip(Pickupable pickupable, GameObject owner, String slot)
        {
            String ownerGuid = GuidHelper.GetGuid(owner);
            Vector3 ownerPos = owner.transform.position;
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);
            
            EquipmentAddItem equip = new EquipmentAddItem(packetSender.PlayerId, ownerGuid, slot, bytes, ownerPos);
            packetSender.Send(equip);
        }

        public void Unequip(Pickupable pickupable, GameObject owner, String slot)
        {
            String itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            String ownerGuid = GuidHelper.GetGuid(owner);
            Vector3 ownerPos = owner.transform.position;

            EquipmentRemoveItem removeEquipment = new EquipmentRemoveItem(packetSender.PlayerId, ownerGuid, slot, itemGuid, ownerPos);
            packetSender.Send(removeEquipment);
        }
    }
}
