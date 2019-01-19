using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class ExosuitModulesEvent
    {
        private readonly IPacketSender packetSender;

        public ExosuitModulesEvent(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastTorpedoLaunch(TechType techType, int slotID, Exosuit instance)
        {
            string Guid = GuidHelper.GetGuid(instance.gameObject);
            TorpedoType torpedoType = null;
            ItemsContainer storageInSlot = instance.GetStorageInSlot(slotID, TechType.ExosuitTorpedoArmModule);
            Log.Info("TORPEDO ARM +" + storageInSlot);

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
                ExosuitModulesAction Changed = new ExosuitModulesAction(techType, slotID, Guid, Player.main.camRoot.GetAimingTransform().forward, Player.main.camRoot.GetAimingTransform().rotation);
                packetSender.Send(Changed);
            }
        }
    }
}
