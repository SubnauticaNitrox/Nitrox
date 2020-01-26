using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;

namespace NitroxClient.GameLogic
{
    public class SeamothModulesEvent
    {
        private readonly IPacketSender packetSender;

        public SeamothModulesEvent(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastTorpedoLaunch(TechType techType, int slotID, SeaMoth instance)
        {
            NitroxId id = NitroxEntity.GetId(instance.gameObject);
            TorpedoType torpedoType = null;
            ItemsContainer storageInSlot = instance.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);

            for (int i = 0; i < instance.torpedoTypes.Length; i++)
            {
                if (storageInSlot.Contains(instance.torpedoTypes[i].techType))
                {
                    torpedoType = instance.torpedoTypes[i];
                    break;
                }
            }

            if (torpedoType != null) // Dont send packet if torpedo storage is empty
            {
                SeamothModulesAction Changed = new SeamothModulesAction(techType.Model(), slotID, id, Player.main.camRoot.GetAimingTransform().forward, Player.main.camRoot.GetAimingTransform().rotation);
                packetSender.Send(Changed);
            }
        }

        public void BroadcastElectricalDefense(TechType techType, int slotID, SeaMoth instance)
        {
            NitroxId id = NitroxEntity.GetId(instance.gameObject);
            if (techType == TechType.SeamothElectricalDefense)
            {
                SeamothModulesAction Changed = new SeamothModulesAction(techType.Model(), slotID, id, Player.main.camRoot.GetAimingTransform().forward, Player.main.camRoot.GetAimingTransform().rotation);
                packetSender.Send(Changed);
            }
        }
    }
}
