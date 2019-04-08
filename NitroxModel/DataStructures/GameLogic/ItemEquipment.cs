using ProtoBufNet;
using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class EquippedItemData : ItemData
    {
        [ProtoMember(1)]
        public string Slot { get; }

        [ProtoMember(2)]
        public TechType TechType { get; }

        public EquippedItemData()
        {
            // For serialization
        }

        public EquippedItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData, string slot, TechType techType) : base(containerId, itemId, serializedData)
        {
            Slot = slot;
            TechType = techType;
        }

        public override string ToString()
        {
            return "[EquippedItemData ContainerGuid: " + ContainerId + "Id: " + ItemId + " Slot: " + Slot + " TechType: " + TechType + "]";
        }
    }
}
