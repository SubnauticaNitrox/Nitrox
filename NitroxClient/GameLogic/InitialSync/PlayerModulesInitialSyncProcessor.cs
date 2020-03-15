using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
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
