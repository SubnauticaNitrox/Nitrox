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

        public EquippedItemData(string containerGuid, string guid, byte[] serializedData, string slot, TechType techType) : base(containerGuid, guid, serializedData)
        {
            Slot = slot;
            TechType = techType;
        }

        public override string ToString()
        {
            return "[EquippedItemData ContainerGuid: " + ContainerGuid + "Guid: " + Guid + " Slot: " + Slot + " TechType: " + TechType + "]";
        }
    }
}
