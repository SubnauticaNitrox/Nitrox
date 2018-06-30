using ProtoBuf;
using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    [ProtoInclude(50, typeof(EquippedItemData))]
    public class ItemData
    {
        [ProtoMember(1)]
        public string ContainerGuid { get; }

        [ProtoMember(2)]
        public string Guid { get; }

        [ProtoMember(3)]
        public byte[] SerializedData { get; }

        public ItemData(string containerGuid, string guid, byte[] serializedData)
        {
            ContainerGuid = containerGuid;
            Guid = guid;
            SerializedData = serializedData;
        }

        public override string ToString()
        {
            return "[ItemData ContainerGuid: " + ContainerGuid + "Guid: " + Guid + "]";
        }
    }
}
