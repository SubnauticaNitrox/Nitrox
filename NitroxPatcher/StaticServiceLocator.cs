using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher
{
    public static class StaticServiceLocator
    {
        public static Building Building;
        public static Crafting Crafting;
        public static EquipmentSlots EquipmentSlots;

        public static void InitializeStaticDependencies()
        {
            Building = NitroxServiceLocator.LocateService<Building>();
            Crafting = NitroxServiceLocator.LocateService<Crafting>();
            EquipmentSlots = NitroxServiceLocator.LocateService<EquipmentSlots>();
        }
    }
}
