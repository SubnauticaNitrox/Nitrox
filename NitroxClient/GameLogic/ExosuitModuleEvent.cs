using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class ExosuitModuleEvent
{
    public static readonly int useToolAnimation = Animator.StringToHash("use_tool");
    public static readonly int bashAnimation = Animator.StringToHash("bash");

    private readonly IPacketSender packetSender;
    private readonly SimulationOwnership simulationOwnership;

    public ExosuitModuleEvent(IPacketSender packetSender, SimulationOwnership simulationOwnership)
    {
        this.packetSender = packetSender;
        this.simulationOwnership = simulationOwnership;
    }


    public void BroadcastClawUse(Exosuit exosuit, ExosuitClawArm clawArm, float cooldown)
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

        BroadcastArmAction(TechType.ExosuitClawArmModule, exosuit, clawArm, action);
    }

    public void BroadcastArmAction(TechType techType, Exosuit exosuit, IExosuitArm exosuitArm, ExosuitArmAction armAction)
    {
        if (exosuit.TryGetIdOrWarn(out NitroxId id) && simulationOwnership.HasAnyLockType(id))
        {
            ExosuitArmActionPacket packet = new(techType, id, GetArmSide(exosuitArm), armAction);
            packetSender.Send(packet);
        }
    }

    public static Exosuit.Arm GetArmSide(IExosuitArm arm)
    {
        return arm.GetGameObject().transform.localScale.x > 0 ? Exosuit.Arm.Left : Exosuit.Arm.Right;
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
                Log.Error($"Drill arm got an arm action it should not get: {armAction}");
                break;
        }
    }

    public static void UseGrappling(ExosuitGrapplingArm grapplingArm, ExosuitArmAction armAction)
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

                Utils.PlayFMODAsset(grapplingArm.shootSound, grapplingArm.front, 15f);
                grapplingArm.grapplingStartPos = componentInParent.transform.position;
                break;
            }
            default:
                Log.Error($"Grappling arm got an arm action it should not get: {armAction}");
                break;
        }
    }

    public static void UsePropulsion(ExosuitPropulsionArm propulsionArm, ExosuitArmAction armAction)
    {
        switch (armAction)
        {
            case ExosuitArmAction.START_USE_TOOL:
                propulsionArm.propulsionCannon.animator.SetBool(useToolAnimation, true);
                break;
            case ExosuitArmAction.END_USE_TOOL:
                propulsionArm.propulsionCannon.animator.SetBool(useToolAnimation, false);
                break;
            default:
                Log.Error($"Propulsion arm got an arm action it should not get: {armAction}");
                break;
        }
    }

    public static void UseTorpedo(ExosuitTorpedoArm torpedoArm, ExosuitArmAction armAction)
    {
        switch (armAction)
        {
            case ExosuitArmAction.START_USE_TOOL:
                torpedoArm.animator.SetBool(useToolAnimation, true);
                break;
            case ExosuitArmAction.END_USE_TOOL:
                torpedoArm.animator.SetBool(useToolAnimation, false);
                break;
            default:
                Log.Error($"Torpedo arm got an arm action it should not get: {armAction}");
                break;

        }
    }
}
