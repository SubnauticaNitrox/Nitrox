using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
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
            try
            {
                GameObject rightArmGameObject = rightArm.GetGameObject();
                rightArmGameObject.SetNewGuid(exosuitModel.RightArmGuid);
                GameObject leftArmGameObject = leftArm.GetGameObject();
                leftArmGameObject.SetNewGuid(exosuitModel.LeftArmGuid);
            } catch (Exception e)
            {
                Log.Warn("Got error setting arm GameObjects. This is probably due to docking sync and can be ignored" + e.Message);
            }
            Log.Debug("Spawn exosuit arms for: " + Guid);
        }

        public void BroadcastClawUse(ExosuitClawArm clawArm, float cooldown)
        {
            string guid = GuidHelper.GetGuid(clawArm.gameObject);
            ExosuitArmAction action;
            if (cooldown == clawArm.cooldownPickup)
            {
                action = ExosuitArmAction.pickup;
            }
            else if (cooldown == clawArm.cooldownPunch)
            {
                action = ExosuitArmAction.punch;
            }
            else
            {
                Log.Error("Cooldown time does not match pickup or punch time");
                return;
            }
            ExosuitArmActionPacket packet = new ExosuitArmActionPacket(TechType.ExosuitClawArmModule, guid, action);
            packetSender.Send(packet);
        }

        public void UseClaw(ExosuitClawArm clawArm,ExosuitArmAction armAction)
        {
            if (armAction == ExosuitArmAction.pickup)
            {
                clawArm.animator.SetTrigger("use_tool");
            }
            else
            {
                clawArm.animator.SetTrigger("bash");
                clawArm.fxControl.Play(0);
            }
        }
    }
}
