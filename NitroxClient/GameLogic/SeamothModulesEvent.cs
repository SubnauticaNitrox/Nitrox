using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

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
                Transform aimingTransform = Player.main.camRoot.GetAimingTransform();
                SeamothModulesAction changed = new SeamothModulesAction(techType.ToDto(), slotID, id, aimingTransform.forward.ToDto(), aimingTransform.rotation.ToDto());
                packetSender.Send(changed);
            }
        }

        public void BroadcastElectricalDefense(TechType techType, int slotID, SeaMoth instance)
        {
            NitroxId id = NitroxEntity.GetId(instance.gameObject);
            if (techType == TechType.SeamothElectricalDefense)
            {
                Transform aimingTransform = Player.main.camRoot.GetAimingTransform();
                SeamothModulesAction changed = new SeamothModulesAction(techType.ToDto(), slotID, id, aimingTransform.forward.ToDto(), aimingTransform.rotation.ToDto());
                packetSender.Send(changed);
            }
        }
    }
}
