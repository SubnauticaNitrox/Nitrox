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
            BroadcastArmAction(TechType.ExosuitClawArmModule, clawArm, action);
        }

        public void UseClaw(ExosuitClawArm clawArm, ExosuitArmAction armAction)
        {
            if (armAction == ExosuitArmAction.pickup)
            {
                clawArm.animator.SetTrigger("use_tool");
            }
            else if(armAction == ExosuitArmAction.punch)
            {
                clawArm.animator.SetTrigger("bash");
                clawArm.fxControl.Play(0);
            }
        }        

        public void UseDrill(ExosuitDrillArm drillArm, ExosuitArmAction armAction)
        {
            if(armAction == ExosuitArmAction.startUseTool)
            {
                drillArm.animator.SetBool("use_tool", true);
                drillArm.loop.Play();
            }
            else if (armAction == ExosuitArmAction.endUseTool)
            {
                drillArm.animator.SetBool("use_tool", false);
                drillArm.ReflectionCall("StopEffects");
            }
            else
            {
                Log.Error("Drill arm got an arm action he should not get: " + armAction);
            }
        }       

        public void BroadcastArmAction(TechType techType, IExosuitArm exosuitArm, ExosuitArmAction armAction, Optional<Vector3> opVector = null)
        {
            string guid = GuidHelper.GetGuid(exosuitArm.GetGameObject());
            
            ExosuitArmActionPacket packet = new ExosuitArmActionPacket(techType, guid, armAction, opVector);
            packetSender.Send(packet);
        }        

        public void UseGrappling(ExosuitGrapplingArm grapplingArm, ExosuitArmAction armAction, Optional<Vector3> opHitVector)
        {
            if (armAction == ExosuitArmAction.endUseTool)
            {
                grapplingArm.animator.SetBool("use_tool", false);
                grapplingArm.ReflectionCall("ResetHook");                
            }
            else if (armAction == ExosuitArmAction.onHit)
            {
                grapplingArm.animator.SetBool("use_tool", true);
                if (!grapplingArm.rope.isLaunching)
                {
                    grapplingArm.rope.LaunchHook(35f);
                }

                GrapplingHook hook = (GrapplingHook)grapplingArm.ReflectionGet("hook");

                hook.transform.parent = null;
                hook.transform.position = grapplingArm.front.transform.position;
                hook.SetFlying(true);
                Exosuit componentInParent = grapplingArm.GetComponentInParent<Exosuit>();

                
                if(opHitVector.IsEmpty())
                {
                    Log.Error("No vector given that contains the hook direction");
                    return;
                }
                
                hook.rb.velocity = opHitVector.Get();
                global::Utils.PlayFMODAsset(grapplingArm.shootSound, grapplingArm.front, 15f);
                grapplingArm.ReflectionSet("grapplingStartPos", componentInParent.transform.position);
            }
            else
            {
                Log.Error("Grappling arm got an arm action he should not get: " + armAction);
            }
        }
    }
}
