using System.Collections.Generic;
using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.MonoBehaviours.Gui.HUD;
using Nitrox.Model.DataStructures;
using Story;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PlayerCinematicController_StartCinematicMode_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.StartCinematicMode(default));
    private static bool skipPrefix;

    public static bool Prefix(PlayerCinematicController __instance)
    {
        if (skipPrefix)
        {
            return true;
        }

        if (__instance.cinematicModeActive)
        {
            return true;
        }

        if (!Player.main || !Player.main.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (!__instance.TryGetComponent(out MultiplayerCinematicController multiplayerCinematicController))
        {
            return true;
        }

        // Get or find the NitroxEntity from the cinematic controller's parent hierarchy
        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            return true;
        }

        NitroxId lockId = BedLockId.Resolve(entity, __instance);

        // Check if we already have the lock
        if (Resolve<SimulationOwnership>().HasExclusiveLock(lockId))
        {
            multiplayerCinematicController.CallAllCinematicModeEnd();
            int identifier = __instance.gameObject.GetHierarchyPath(entity.gameObject).GetHashCode();
            Dictionary<string, bool> animationParameters = CaptureAnimationParameters(__instance, entity.gameObject);
            Resolve<PlayerCinematics>().StartCinematicMode(Resolve<LocalPlayer>().SessionId.Value, entity.Id, identifier, __instance.playerViewAnimationName, animationParameters);
            return true;
        }

        // Request exclusive lock to prevent multiple players from using the same cinematic simultaneously
        CinematicInteraction context = new(__instance, entity, multiplayerCinematicController);
        LockRequest<CinematicInteraction> lockRequest = new(lockId, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);
        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAcquired, CinematicInteraction context)
    {
        if (lockAcquired)
        {
            context.MultiplayerController.CallAllCinematicModeEnd();
            int identifier = context.Controller.gameObject.GetHierarchyPath(context.Entity.gameObject).GetHashCode();
            Dictionary<string, bool> animationParameters = CaptureAnimationParameters(context.Controller, context.Entity.gameObject);
            
            skipPrefix = true;
            context.Controller.StartCinematicMode(Player.main);
            skipPrefix = false;
            
            Resolve<PlayerCinematics>().StartCinematicMode(Resolve<LocalPlayer>().SessionId.Value, context.Entity.Id, identifier, context.Controller.playerViewAnimationName, animationParameters);
        }
        else
        {
            context.Controller.gameObject.AddComponent<DenyOwnershipHand>();
            ErrorMessage.AddMessage(Language.main.Get("Nitrox_DenyOwnershipHand"));
        }
    }

    /// <summary>
    /// Captures animation parameters for cinematics that require them.
    /// Currently handles the gun terminal which needs firstUse and cured states.
    /// </summary>
    private static Dictionary<string, bool> CaptureAnimationParameters(PlayerCinematicController cinematicController, GameObject entityRoot)
    {
        Dictionary<string, bool> parameters = new();

        // Gun terminal: needs firstUse and cured parameters for correct animation selection
        if (cinematicController.playerViewAnimationName == "precursor_deactivate_gun" &&
            TryFindTerminalComponent(entityRoot, cinematicController, out PrecursorDisableGunTerminal terminal))
        {
            bool playerCured = StoryGoalManager.main != null &&
                               terminal.onPlayerCuredGoal != null &&
                               StoryGoalManager.main.IsGoalComplete(terminal.onPlayerCuredGoal.key);

            // Terminal animator parameters
            parameters["first_use"] = terminal.firstUse;
            parameters["cured"] = playerCured;
            // Player animator parameters
            parameters["using_tool_first"] = terminal.firstUse;
        }

        return parameters;
    }

    /// <summary>
    /// Finds the PrecursorDisableGunTerminal component using multiple search strategies.
    /// </summary>
    private static bool TryFindTerminalComponent(GameObject entityRoot, PlayerCinematicController cinematicController, out PrecursorDisableGunTerminal terminal)
    {
        // Try entity root first, then its children, then the controller's parent
        terminal = entityRoot.GetComponent<PrecursorDisableGunTerminal>();
        if (!terminal)
        {
            terminal = entityRoot.GetComponentInChildren<PrecursorDisableGunTerminal>();
        }
        if (!terminal)
        {
            terminal = cinematicController.GetComponentInParent<PrecursorDisableGunTerminal>();
        }
        if (!terminal)
        {
            Log.Warn("Could not find PrecursorDisableGunTerminal component for gun terminal cinematic");
        }
        return terminal;
    }

    private readonly struct CinematicInteraction(PlayerCinematicController controller, NitroxEntity entity, MultiplayerCinematicController multiplayerController) : LockRequestContext
    {
        public PlayerCinematicController Controller { get; } = controller;
        public NitroxEntity Entity { get; } = entity;
        public MultiplayerCinematicController MultiplayerController { get; } = multiplayerController;
    }
}
