using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
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
        private readonly Vehicles vehicles;

        public ExosuitModuleEvent(IPacketSender packetSender, IMultiplayerSession multiplayerSession, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.multiplayerSession = multiplayerSession;
            this.vehicles = vehicles;
        }

        public void BroadcastSpawnedArm(Exosuit exo)
        {
            string Guid = GuidHelper.GetGuid(exo.gameObject);
            if (!string.IsNullOrEmpty(Guid))
            {

                ExosuitModel exosuitModel = vehicles.GetVehicles<ExosuitModel>(Guid);
               
                IExosuitArm spawnedRArm = (IExosuitArm)exo.ReflectionGet("rightArm");
                IExosuitArm spawnedLArm = (IExosuitArm)exo.ReflectionGet("leftArm");

                GameObject spawnedRArmOb = spawnedRArm.GetGameObject();
                spawnedRArmOb.SetNewGuid(exosuitModel.RightArmGuid);

                GameObject spawnedLArmOb = spawnedLArm.GetGameObject();
                spawnedLArmOb.SetNewGuid(exosuitModel.LeftArmGuid);

                ExosuitSpawnedArmAction Changed = new ExosuitSpawnedArmAction(Guid, exosuitModel.LeftArmGuid, exosuitModel.RightArmGuid);
                packetSender.Send(Changed);
            }
        }
    }
}
