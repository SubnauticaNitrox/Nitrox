using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class EquippedItemData : ItemData
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual string Slot { get; protected set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual NitroxTechType TechType { get; protected set; }

        protected EquippedItemData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public EquippedItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData, string slot, NitroxTechType techType) : base(containerId, itemId, serializedData)
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
