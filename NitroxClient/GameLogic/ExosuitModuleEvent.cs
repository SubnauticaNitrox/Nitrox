using NitroxClient.Communication.Abstract;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class ExosuitModuleEvent
    {
        private readonly IPacketSender packetSender;

        public ExosuitModuleEvent(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastClawUse(ExosuitClawArm clawArm, float cooldown)
        {
            ExosuitArmAction action;

            // If cooldown of claw arm matches pickup cooldown, the exosuit arm performed a pickup action
            if (cooldown == clawArm.cooldownPickup)
            {
                action = ExosuitArmAction.START_USE_TOOL;
            } // Else if it matches the punch cooldown, it has punched something (or nothing but water, who knows)
            else if (cooldown == clawArm.cooldownPunch)
            {
                action = ExosuitArmAction.ALT_HIT;
            }
            else
            {
                Log.Error("Cooldown time does not match pickup or punch time");
                return;
            }

            BroadcastArmAction(TechType.ExosuitClawArmModule, clawArm, action, null, null);
        }

        public void UseClaw(ExosuitClawArm clawArm, ExosuitArmAction armAction)
        {
            if (armAction == ExosuitArmAction.START_USE_TOOL)
            {
                clawArm.animator.SetTrigger("use_tool");
            }
            else if (armAction == ExosuitArmAction.ALT_HIT)
            {
                clawArm.animator.SetTrigger("bash");
                clawArm.fxControl.Play(0);
            }
        }

        public void UseDrill(ExosuitDrillArm drillArm, ExosuitArmAction armAction)
        {
            if (armAction == ExosuitArmAction.START_USE_TOOL)
            {
                drillArm.animator.SetBool("use_tool", true);
                drillArm.loop.Play();
            }
            else if (armAction == ExosuitArmAction.END_USE_TOOL)
            {
                drillArm.animator.SetBool("use_tool", false);
                drillArm.StopEffects();
            }
            else
            {
                Log.Error($"Drill arm got an arm action he should not get: {armAction}");
            }
        }

        public void BroadcastArmAction(TechType techType, IExosuitArm exosuitArm, ExosuitArmAction armAction, Vector3? opVector, Quaternion? opRotation)
        {
            if (exosuitArm.GetGameObject().TryGetIdOrWarn(out NitroxId id))
            {
                ExosuitArmActionPacket packet = new(techType, id, armAction, opVector?.ToDto(), opRotation?.ToDto());
                packetSender.Send(packet);
            }
        }

        public void BroadcastArmAction(TechType techType, IExosuitArm exosuitArm, ExosuitArmAction armAction)
        {
            if (exosuitArm.GetGameObject().TryGetIdOrWarn(out NitroxId id))
            {
                ExosuitArmActionPacket packet = new(techType, id, armAction, null, null);
                packetSender.Send(packet);
            }
        }

        public void UseGrappling(ExosuitGrapplingArm grapplingArm, ExosuitArmAction armAction, Vector3? opHitVector)
        {
            if (armAction == ExosuitArmAction.END_USE_TOOL)
            {
                grapplingArm.animator.SetBool("use_tool", false);
                grapplingArm.ResetHook();
            }
            else if (armAction == ExosuitArmAction.START_USE_TOOL)
            {
                grapplingArm.animator.SetBool("use_tool", true);
                if (!grapplingArm.rope.isLaunching)
                {
                    grapplingArm.rope.LaunchHook(35f);
                }

                GrapplingHook hook = grapplingArm.hook;

                hook.transform.parent = null;
                hook.transform.position = grapplingArm.front.transform.position;
                hook.SetFlying(true);
                Exosuit componentInParent = grapplingArm.GetComponentInParent<Exosuit>();

                if (!opHitVector.HasValue)
                {
                    Log.Error("No vector given that contains the hook direction");
                    return;
                }

                hook.rb.velocity = opHitVector.Value;
                Utils.PlayFMODAsset(grapplingArm.shootSound, grapplingArm.front, 15f);
                grapplingArm.grapplingStartPos = componentInParent.transform.position;
            }
            else
            {
                Log.Error($"Grappling arm got an arm action he should not get: {armAction}");
            }
        }

        public void UseTorpedo(ExosuitTorpedoArm torpedoArm, ExosuitArmAction armAction, Vector3? opVector, Quaternion? opRotation)
        {
            if (armAction == ExosuitArmAction.START_USE_TOOL || armAction == ExosuitArmAction.ALT_HIT)
            {
                if (!opVector.HasValue || !opRotation.HasValue)
                {
                    Log.Error("Torpedo arm action shoot: no vector or rotation present");
                    return;
                }

                Vector3 forward = opVector.Value;
                Quaternion rotation = opRotation.Value;
                Transform silo;
                if (armAction == ExosuitArmAction.START_USE_TOOL)
                {
                    silo = torpedoArm.siloFirst;
                }
                else
                {
                    silo = torpedoArm.siloSecond;
                }

                ItemsContainer container = torpedoArm.container;
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
                SeamothTorpedo component2 = gameObject.GetComponent<SeamothTorpedo>();
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
            else if (armAction == ExosuitArmAction.END_USE_TOOL)
            {
                torpedoArm.animator.SetBool("use_tool", false);
            }
            else
            {
                Log.Error($"Torpedo arm got an arm action he should not get: {armAction}");
            }
        }
    }
}
