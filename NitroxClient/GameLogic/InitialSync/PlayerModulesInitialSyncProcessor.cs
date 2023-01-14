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

            DependentProcessors.Add(typeof(GlobalRootInitialSyncProcessor)); // modules can be inside vehicles in global root
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor));
            DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor));
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            equipmentSlots.AddItems(packet.Modules);
            yield return null;
        }
    }
}
