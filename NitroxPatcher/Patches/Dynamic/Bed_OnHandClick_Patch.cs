using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours.Gui.HUD;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Intercepts bed click to request lock and send bed animation packet for remote players.
/// </summary>
public sealed partial class Bed_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bed t) => t.OnHandClick(default(GUIHand)));
    private static bool skipPrefix;

    public static bool Prefix(Bed __instance, GUIHand hand)
    {
        if (skipPrefix)
        {
            return true;
        }

        if (!__instance.TryGetIdOrWarn(out NitroxId bedId))
        {
            return true;
        }

        if (Resolve<SimulationOwnership>().HasExclusiveLock(bedId))
        {
            Log.Debug($"Already have an exclusive lock on the bed: {bedId}");
            return true;
        }

        HandInteraction<Bed> context = new(__instance, hand);
        LockRequest<HandInteraction<Bed>> lockRequest = new(bedId, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId bedId, bool lockAcquired, HandInteraction<Bed> context)
    {
        Bed bed = context.Target;

        if (lockAcquired)
        {
            skipPrefix = true;
            bed.OnHandClick(context.GuiHand);
            skipPrefix = false;

            // Determine which side was used based on the cinematic controller that was set in OnHandClick
            string animationKey = bed.cinematicController == bed.leftLieDownCinematicController 
                ? "bed_down_left" 
                : "bed_down_right";

            Resolve<IPacketSender>().Send(new BedEnterAnimation(Resolve<LocalPlayer>().SessionId.Value, bedId, animationKey));
        }
        else
        {
            bed.gameObject.AddComponent<DenyOwnershipHand>();
            ErrorMessage.AddMessage("Another player is using this bed");
        }
    }
}
