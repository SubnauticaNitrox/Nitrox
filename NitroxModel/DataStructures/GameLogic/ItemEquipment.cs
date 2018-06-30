using ProtoBuf;
using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class ItemEquipment
    {
        [ProtoMember(1)]
        public string ContainerGuid { get; }

        [ProtoMember(2)]
        public string Guid { get; }

        [ProtoMember(3)]
        public byte[] SerializedData { get; }

        [ProtoMember(4)]
        public string Slot { get; }

        public ItemEquipment(string containerGuid, string guid, byte[] serializedData, string slot)
        {
            Slot = slot;
            ContainerGuid = containerGuid;
            Guid = guid;
            SerializedData = serializedData;
        }

        public override string ToString()
        {
            return "[ItemDataEquipment ContainerGuid: " + ContainerGuid + "Guid: " + Guid + " Slot: " + Slot + "]";
        }
    }
}
