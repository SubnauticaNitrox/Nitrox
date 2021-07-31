using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ModuleAdded : Packet
    {
        public EquippedItemData EquippedItemData { get; }
        public bool PlayerModule { get; }

        public ModuleAdded(EquippedItemData equippedItemData, bool playerModule)
        {
            EquippedItemData = equippedItemData;
            PlayerModule = playerModule;
        }
    }
}
