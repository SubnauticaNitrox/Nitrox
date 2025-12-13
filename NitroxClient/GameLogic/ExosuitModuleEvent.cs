using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class ExosuitModuleEvent
{
    public static readonly int UseToolAnimation = Animator.StringToHash("use_tool");
    public static readonly int BashAnimation = Animator.StringToHash("bash");

    private readonly IPacketSender packetSender;
    private readonly SimulationOwnership simulationOwnership;

    public ExosuitModuleEvent(IPacketSender packetSender, SimulationOwnership simulationOwnership)
    {
        this.packetSender = packetSender;
        this.simulationOwnership = simulationOwnership;
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
                clawArm.animator.SetTrigger(UseToolAnimation);
                break;
            case ExosuitArmAction.ALT_HIT:
                clawArm.animator.SetTrigger(BashAnimation);
                clawArm.fxControl.Play(0);
                break;
        }
    }

    public static void UseDrill(ExosuitDrillArm drillArm, ExosuitArmAction armAction)
    {
        switch (armAction)
        {
            case ExosuitArmAction.START_USE_TOOL:
                drillArm.animator.SetBool(UseToolAnimation, true);
                drillArm.loop.Play();
                break;
            case ExosuitArmAction.END_USE_TOOL:
                drillArm.animator.SetBool(UseToolAnimation, false);
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
                grapplingArm.animator.SetBool(UseToolAnimation, false);
                grapplingArm.ResetHook();
                break;
            case ExosuitArmAction.START_USE_TOOL:
                grapplingArm.animator.SetBool(UseToolAnimation, true);
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
                propulsionArm.propulsionCannon.animator.SetBool(UseToolAnimation, true);
                break;
            case ExosuitArmAction.END_USE_TOOL:
                propulsionArm.propulsionCannon.animator.SetBool(UseToolAnimation, false);
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
                torpedoArm.animator.SetBool(UseToolAnimation, true);
                break;
            case ExosuitArmAction.END_USE_TOOL:
                torpedoArm.animator.SetBool(UseToolAnimation, false);
                break;
            default:
                Log.Error($"Torpedo arm got an arm action it should not get: {armAction}");
                break;

        }
    }
}
