using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PilotingChair_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PilotingChair t) => t.OnHandClick(default(GUIHand)));

    private static bool skipPrefix;

    public static bool Prefix(PilotingChair __instance, GUIHand hand)
    {
        if (skipPrefix)
        {
            return true;
        }

        SubRoot subRoot = __instance.GetComponentInParent<SubRoot>();
        Validate.NotNull(subRoot, "PilotingChair cannot find it's corresponding SubRoot!");

        if (!subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            return true;
        }

        if (Resolve<SimulationOwnership>().HasExclusiveLock(id))
        {
            Log.Debug($"Already have an exclusive lock on the piloting chair: {id}");
            return true;
        }

        HandInteraction<PilotingChair> context = new(__instance, hand);
        LockRequest<HandInteraction<PilotingChair>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<PilotingChair> context)
    {
        PilotingChair pilotingChair = context.Target;

        if (lockAquired)
        {
            skipPrefix = true;
            pilotingChair.OnHandClick(context.GuiHand);
            skipPrefix = false;
        }
        else
        {
            pilotingChair.gameObject.AddComponent<DenyOwnershipHand>();
            pilotingChair.isValidHandTarget = false;
        }
    }
}
