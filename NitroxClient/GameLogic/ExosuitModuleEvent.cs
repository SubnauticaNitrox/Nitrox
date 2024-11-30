using NitroxClient.Communication.Abstract;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class ExosuitModuleEvent
{
    private static readonly int useToolAnimation = Animator.StringToHash("use_tool");
    private static readonly int bashAnimation = Animator.StringToHash("bash");

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

    public static void UseClaw(ExosuitClawArm clawArm, ExosuitArmAction armAction)
    {
        switch (armAction)
        {
            case ExosuitArmAction.START_USE_TOOL:
                clawArm.animator.SetTrigger(useToolAnimation);
                break;
            case ExosuitArmAction.ALT_HIT:
                clawArm.animator.SetTrigger(bashAnimation);
                clawArm.fxControl.Play(0);
                break;
        }
    }

    public static void UseDrill(ExosuitDrillArm drillArm, ExosuitArmAction armAction)
    {
        switch (armAction)
        {
            case ExosuitArmAction.START_USE_TOOL:
                drillArm.animator.SetBool(useToolAnimation, true);
                drillArm.loop.Play();
                break;
            case ExosuitArmAction.END_USE_TOOL:
                drillArm.animator.SetBool(useToolAnimation, false);
                drillArm.StopEffects();
                break;
            default:
                Log.Error($"Drill arm got an arm action he should not get: {armAction}");
                break;
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

    public static void UseGrappling(ExosuitGrapplingArm grapplingArm, ExosuitArmAction armAction, Vector3? opHitVector)
    {
        switch (armAction)
        {
            case ExosuitArmAction.END_USE_TOOL:
                grapplingArm.animator.SetBool(useToolAnimation, false);
                grapplingArm.ResetHook();
                break;
            case ExosuitArmAction.START_USE_TOOL:
            {
                grapplingArm.animator.SetBool(useToolAnimation, true);
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
                break;
            }
            default:
                Log.Error($"Grappling arm got an arm action he should not get: {armAction}");
                break;
        }
    }
}
