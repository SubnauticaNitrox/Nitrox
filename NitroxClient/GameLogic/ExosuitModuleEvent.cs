using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class ExosuitModuleEvent
    {
        private readonly IPacketSender packetSender;
        private readonly IMultiplayerSession multiplayerSession;

        public ExosuitModuleEvent(IPacketSender packetSender, IMultiplayerSession multiplayerSession)
        {
            this.packetSender = packetSender;
            this.multiplayerSession = multiplayerSession;
        }

        public void BroadcastSpawnedArm(Exosuit exo)
        {
            string Guid = GuidHelper.GetGuid(exo.gameObject);
            if (!string.IsNullOrEmpty(Guid))
            {
                // This needs to be refactored to be done by the serverr, Since Every client will run this and the last one that does will sync all the guids...
                IExosuitArm spawnedRArm = (IExosuitArm)exo.ReflectionGet("rightArm");
                IExosuitArm spawnedLArm = (IExosuitArm)exo.ReflectionGet("leftArm");

                GameObject spawnedRArmOb = spawnedRArm.GetGameObject();
                string rightArmGuid = GuidHelper.GetGuid(spawnedRArmOb);
                spawnedRArmOb.SetNewGuid(rightArmGuid);

                GameObject spawnedLArmOb = spawnedLArm.GetGameObject();
                string leftArmGuid = GuidHelper.GetGuid(spawnedLArmOb);
                spawnedLArmOb.SetNewGuid(leftArmGuid);

                ExosuitSpawnedArmAction Changed = new ExosuitSpawnedArmAction(Guid, leftArmGuid, rightArmGuid);
                packetSender.Send(Changed);
            }
        }
    }
}
