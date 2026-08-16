using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.HUD;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CinematicModeTriggerBase_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((CinematicModeTriggerBase t) => t.OnHandClick(default));
    private static bool skipPrefix;

    public static bool Prefix(CinematicModeTriggerBase __instance, GUIHand hand)
    {
        if (skipPrefix)
        {
            return true;
        }

        // Only intercept hand target triggers
        if (__instance.triggerType != CinematicModeTriggerBase.TriggerType.HandTarget || !__instance.isValidHandTarget)
        {
            return true;
        }

        // If no cinematic controller, let vanilla handle it
        if (!__instance.cinematicController)
        {
            return true;
        }

        // Try to get the NitroxEntity from the cinematic controller's parent hierarchy
        if (!__instance.cinematicController.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            return true;
        }

        // Check if we already have the lock
        if (Resolve<SimulationOwnership>().HasExclusiveLock(entity.Id))
        {
            return true;
        }

        // Request exclusive lock to prevent multiple players from using the same cinematic simultaneously
        CinematicTriggerInteraction context = new(__instance, hand, entity);
        LockRequest<CinematicTriggerInteraction> lockRequest = new(entity.Id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);
        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAcquired, CinematicTriggerInteraction context)
    {
        if (lockAcquired)
        {
            skipPrefix = true;
            context.Trigger.OnHandClick(context.Hand);
            skipPrefix = false;
        }
        else
        {
            context.Trigger.gameObject.AddComponent<DenyOwnershipHand>();
            ErrorMessage.AddMessage("Another player is using this");
        }
    }

    private readonly struct CinematicTriggerInteraction(CinematicModeTriggerBase trigger, GUIHand hand, NitroxEntity entity) : LockRequestContext
    {
        public CinematicModeTriggerBase Trigger { get; } = trigger;
        public GUIHand Hand { get; } = hand;
        public NitroxEntity Entity { get; } = entity;
    }
}
