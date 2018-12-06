using ProtoBuf;
using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class EquippedItemData : ItemData
    {
        [ProtoMember(1)]
        public string Slot { get; }

        public EquippedItemData(string containerGuid, string guid, byte[] serializedData, string slot) : base(containerGuid, guid, serializedData)
        {
            Slot = slot;
        }

        public override string ToString()
        {
            return "[EquippedItemData ContainerGuid: " + ContainerGuid + "Guid: " + Guid + " Slot: " + Slot + "]";
        }
    }
}
