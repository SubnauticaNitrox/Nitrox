﻿using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
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
            NitroxId id = NitroxIdentifier.GetId(exosuit.gameObject);            
            ExosuitModel exosuitModel = vehicles.GetVehicles<ExosuitModel>(id);
            
            IExosuitArm rightArm = (IExosuitArm)exosuit.ReflectionGet("rightArm");            
            IExosuitArm leftArm = (IExosuitArm)exosuit.ReflectionGet("leftArm");  
            
            try
            {
                GameObject rightArmGameObject = rightArm.GetGameObject();
                NitroxIdentifier.SetNewId(rightArmGameObject, exosuitModel.RightArmId);

                GameObject leftArmGameObject = leftArm.GetGameObject();
                NitroxIdentifier.SetNewId(leftArmGameObject, exosuitModel.LeftArmId);
            }
            catch (Exception e)
            {
                Log.Warn("Got error setting arm GameObjects. This is probably due to docking sync and can be ignored\nErromessage: " + e.Message + "\n" + e.StackTrace);
            }

            Log.Debug("Spawn exosuit arms for: " + id);
        }                

        public void BroadcastClawUse(ExosuitClawArm clawArm, float cooldown)
        {
            NitroxId id = NitroxIdentifier.GetId(clawArm.gameObject);
            ExosuitArmAction action;

            // If cooldown of claw arm matches pickup cooldown, the exosuit arm performed a pickup action
            if (cooldown == clawArm.cooldownPickup)
            {
                action = ExosuitArmAction.startUseTool;
            } // Else if it matches the punch cooldown, it has punched something (or nothing but water, who knows)
            else if (cooldown == clawArm.cooldownPunch)
            {
                action = ExosuitArmAction.altHit;
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
            if (armAction == ExosuitArmAction.startUseTool)
            {
                clawArm.animator.SetTrigger("use_tool");
            }
            else if(armAction == ExosuitArmAction.altHit)
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

        public void BroadcastArmAction(TechType techType, IExosuitArm exosuitArm, ExosuitArmAction armAction, Optional<Vector3> opVector = null, Optional<Quaternion> opRotation = null)
        {
            NitroxId id = NitroxIdentifier.GetId(exosuitArm.GetGameObject());            
            ExosuitArmActionPacket packet = new ExosuitArmActionPacket(techType, id, armAction, opVector, opRotation);
            packetSender.Send(packet);
        }        

        public void UseGrappling(ExosuitGrapplingArm grapplingArm, ExosuitArmAction armAction, Optional<Vector3> opHitVector)
        {
            if (armAction == ExosuitArmAction.endUseTool)
            {
                grapplingArm.animator.SetBool("use_tool", false);
                grapplingArm.ReflectionCall("ResetHook");                
            }
            else if (armAction == ExosuitArmAction.startUseTool)
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

        public void UseTorpedo(ExosuitTorpedoArm torpedoArm, ExosuitArmAction armAction, Optional<Vector3> opVector, Optional<Quaternion> opRotation)
        {            
            if (armAction == ExosuitArmAction.startUseTool || armAction == ExosuitArmAction.altHit)
            {
                if(opVector.IsEmpty() || opRotation.IsEmpty())
                {
                    Log.Error("Torpedo arm action shoot: no vector or rotation present");
                    return;
                }
                Vector3 forward = opVector.Get();
                Quaternion rotation = opRotation.Get();
                Transform silo = default(Transform);
                if(armAction == ExosuitArmAction.startUseTool)
                {
                    silo = torpedoArm.siloFirst;
                }
                else
                {
                    silo = torpedoArm.siloSecond;
                }
                ItemsContainer container = (ItemsContainer)torpedoArm.ReflectionGet("container");
                Exosuit exosuit = torpedoArm.GetComponentInParent<Exosuit>();
                TorpedoType[] torpedoTypes = exosuit.torpedoTypes;

                TorpedoType torpedoType = null;
                for (int i = 0; i < torpedoTypes.Length; i++)
                {
                    if (container.Contains(torpedoTypes[i].techType))
                    {
                        torpedoType = torpedoTypes[i];
                        break;
                    }
                }

                // Copied from SeamothModuleActionProcessor. We need to synchronize both methods
                GameObject gameObject = UnityEngine.Object.Instantiate(torpedoType.prefab);
                Transform component = gameObject.GetComponent<Transform>();
                SeamothTorpedo component2 = gameObject.GetComponent<SeamothTorpedo>();
                Vector3 zero = Vector3.zero;
                Rigidbody componentInParent = silo.GetComponentInParent<Rigidbody>();
                Vector3 rhs = (!(componentInParent != null)) ? Vector3.zero : componentInParent.velocity;
                float speed = Vector3.Dot(forward, rhs);
                component2.Shoot(silo.position, rotation, speed, -1f);

                torpedoArm.animator.SetBool("use_tool", true);
                if (container.count == 0)
                {
                    Utils.PlayFMODAsset(torpedoArm.torpedoDisarmed, torpedoArm.transform, 1f);
                }

            }
            else if (armAction == ExosuitArmAction.endUseTool)
            {
                torpedoArm.animator.SetBool("use_tool", false);
            }
            else
            {
                Log.Error("Torpedo arm got an arm action he should not get: " + armAction);
            }
        }

    }
}
