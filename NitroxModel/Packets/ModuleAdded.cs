using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ModuleAdded : Packet
    {
        [Index(0)]
        public virtual EquippedItemData EquippedItemData { get; protected set; }
        [Index(1)]
        public virtual bool PlayerModule { get; protected set; }

        private ModuleAdded() { }

        public ModuleAdded(EquippedItemData equippedItemData, bool playerModule)
        {
            EquippedItemData = equippedItemData;
            PlayerModule = playerModule;
        }
    }
}
