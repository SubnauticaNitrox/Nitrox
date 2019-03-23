using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;
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

        public void SpawnedArm(Exosuit exosuit)
        {            
            string Guid = GuidHelper.GetGuid(exosuit.gameObject);
            ExosuitModel exosuitModel = vehicles.GetVehicles<ExosuitModel>(Guid);
               
            IExosuitArm rightArm = (IExosuitArm)exosuit.ReflectionGet("rightArm");
            IExosuitArm leftArm = (IExosuitArm)exosuit.ReflectionGet("leftArm");

            GameObject rightArmGameObject = rightArm.GetGameObject();
            rightArmGameObject.SetNewGuid(exosuitModel.RightArmGuid);

            GameObject leftArmGameObject = leftArm.GetGameObject();
            leftArmGameObject.SetNewGuid(exosuitModel.LeftArmGuid);

            Log.Debug("Spawn exosuit arms for: " + Guid);
        }
    }
}
