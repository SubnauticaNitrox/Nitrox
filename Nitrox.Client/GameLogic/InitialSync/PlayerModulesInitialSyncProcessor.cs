using System.Collections;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Model.Packets;

namespace Nitrox.Client.GameLogic.InitialSync
{
    class PlayerModulesInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly EquipmentSlots equipmentSlots;

        public PlayerModulesInitialSyncProcessor(IPacketSender packetSender, EquipmentSlots equipmentSlots)
        {
            this.packetSender = packetSender;
            this.equipmentSlots = equipmentSlots;

            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor));
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor));
            DependentProcessors.Add(typeof(InventoryItemsInitialSyncProcessor));
            DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor));
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {            
            using (packetSender.Suppress<ModuleAdded>())
            {
                equipmentSlots.AddItems(packet.Modules);
                yield return null;
            }
        }
    }
}
