using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.HUD;
using Nitrox.Model.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class UseableDiveHatch_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((UseableDiveHatch t) => t.OnHandClick(default));
    private static bool skipPrefix;

    public static bool Prefix(UseableDiveHatch __instance, GUIHand hand)
    {
        if (skipPrefix || !__instance.enabled)
        {
            return true;
        }

        // Only intercept cinematic interactions (when cinematicController is set)
        PlayerCinematicController cinematicController = null;
        bool isInside = Player.main.IsInsideWalkable() && Player.main.currentWaterPark == null;
        
        if (isInside && !__instance.enterOnly)
        {
            cinematicController = __instance.exitCinematicController;
        }
        else
        {
            cinematicController = __instance.enterCinematicController;
        }

        // If no cinematic controller, let vanilla handle it
        if (!cinematicController)
        {
            return true;
        }

        // Try to get the NitroxEntity from the cinematic controller's parent hierarchy
        if (!cinematicController.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            return true;
        }

        // Check if we already have the lock
        if (Resolve<SimulationOwnership>().HasExclusiveLock(entity.Id))
        {
            return true;
        }

        // Request exclusive lock to prevent multiple players from using the same hatch simultaneously
        HatchInteraction context = new(__instance, hand, cinematicController, entity);
        LockRequest<HatchInteraction> lockRequest = new(entity.Id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);
        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAcquired, HatchInteraction context)
    {
        if (lockAcquired)
        {
            skipPrefix = true;
            context.Hatch.OnHandClick(context.Hand);
            skipPrefix = false;
        }
        else
        {
            context.Hatch.gameObject.AddComponent<DenyOwnershipHand>();
            ErrorMessage.AddMessage("Another player is using this");
        }
    }

    private readonly struct HatchInteraction(UseableDiveHatch hatch, GUIHand hand, PlayerCinematicController controller, NitroxEntity entity) : LockRequestContext
    {
        public UseableDiveHatch Hatch { get; } = hatch;
        public GUIHand Hand { get; } = hand;
        public PlayerCinematicController Controller { get; } = controller;
        public NitroxEntity Entity { get; } = entity;
    }
}
