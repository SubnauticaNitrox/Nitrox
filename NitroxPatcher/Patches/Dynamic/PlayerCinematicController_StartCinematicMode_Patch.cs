using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxClient.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
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
            Log.Info($"[CinematicLock] Skipping prefix (skipPrefix=true) for {__instance.gameObject.name}");
            return true;
        }

        if (__instance.cinematicModeActive)
        {
            Log.Info($"[CinematicLock] Cinematic already active for {__instance.gameObject.name}");
            return true;
        }

        // Defensive check: Ensure Player.main is valid before starting cinematic
        // This prevents race conditions during player initialization or world loading
        if (!Player.main || !Player.main.gameObject.activeInHierarchy)
        {
            Log.Warn($"[CinematicLock] Player.main not ready (exists: {Player.main != null}, active: {(Player.main ? Player.main.gameObject.activeInHierarchy : false)}) - blocking cinematic start to prevent race condition");
            return false;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            Log.Warn($"[CinematicLock] No NitroxEntity for \"{__instance.gameObject.GetFullHierarchyPath()}\" - cinematic will work but won't be locked!");
            return true;
        }

        if (!__instance.TryGetComponent(out MultiplayerCinematicController multiplayerCinematicController))
        {
            Log.Error($"[CinematicLock] No MultiplayerCinematicController for \"{__instance.gameObject.GetFullHierarchyPath()}\" - this shouldn't happen!");
            return true;
        }

        Log.Info($"[CinematicLock] Attempting to start cinematic:");
        Log.Info($"  - GameObject: {__instance.gameObject.name}");
        Log.Info($"  - Entity ID: {entity.Id}");
        Log.Info($"  - Animation: {__instance.playerViewAnimationName}");
        Log.Info($"  - Full Path: {__instance.gameObject.GetFullHierarchyPath()}");

        // Check if we already have the lock
        if (Resolve<SimulationOwnership>().HasExclusiveLock(entity.Id))
        {
            Log.Info($"[CinematicLock] Already have exclusive lock on {entity.Id}, starting cinematic immediately");
            multiplayerCinematicController.CallAllCinematicModeEnd();
            int identifier = __instance.gameObject.GetHierarchyPath(entity.gameObject).GetHashCode();
            Dictionary<string, bool> animationParameters = CaptureAnimationParameters(__instance, entity.gameObject);
            Resolve<PlayerCinematics>().StartCinematicMode(Resolve<LocalPlayer>().SessionId.Value, entity.Id, identifier, __instance.playerViewAnimationName, animationParameters);
            return true;
        }

        Log.Info($"[CinematicLock] Requesting EXCLUSIVE lock for entity {entity.Id}");
        
        // Request exclusive lock to prevent multiple players from using the same cinematic simultaneously
        CinematicInteraction context = new(__instance, entity, multiplayerCinematicController);
        LockRequest<CinematicInteraction> lockRequest = new(entity.Id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);
        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        Log.Info($"[CinematicLock] Lock request sent, waiting for response...");
        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAcquired, CinematicInteraction context)
    {
        Log.Info($"[CinematicLock] Lock response received:");
        Log.Info($"  - Entity ID: {id}");
        Log.Info($"  - Lock Acquired: {lockAcquired}");
        Log.Info($"  - GameObject: {context.Controller.gameObject.name}");
        Log.Info($"  - Animation: {context.Controller.playerViewAnimationName}");

        if (lockAcquired)
        {
            Log.Info($"[CinematicLock] Lock acquired! Starting cinematic for {context.Controller.gameObject.name}");
            
            context.MultiplayerController.CallAllCinematicModeEnd();
            int identifier = context.Controller.gameObject.GetHierarchyPath(context.Entity.gameObject).GetHashCode();
            Dictionary<string, bool> animationParameters = CaptureAnimationParameters(context.Controller, context.Entity.gameObject);
            
            // Let the game's original StartCinematicMode run
            skipPrefix = true;
            context.Controller.StartCinematicMode(Player.main);
            skipPrefix = false;
            
            // Broadcast to other players
            Resolve<PlayerCinematics>().StartCinematicMode(Resolve<LocalPlayer>().SessionId.Value, id, identifier, context.Controller.playerViewAnimationName, animationParameters);
            
            Log.Info($"[CinematicLock] Cinematic started successfully");
        }
        else
        {
            Log.Warn($"[CinematicLock] Lock DENIED for {context.Controller.gameObject.name} - another player is using this cinematic");
            
            // Another player is using this cinematic - show visual feedback
            context.Entity.gameObject.AddComponent<DenyOwnershipHand>();
            ErrorMessage.AddMessage("Another player is using this");
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
        if (cinematicController.playerViewAnimationName == "precursor_deactivate_gun")
        {
            PrecursorDisableGunTerminal terminal = FindTerminalComponent(entityRoot, cinematicController);
            if (terminal)
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
        }

        return parameters;
    }

    /// <summary>
    /// Finds the PrecursorDisableGunTerminal component using multiple search strategies.
    /// </summary>
    private static PrecursorDisableGunTerminal FindTerminalComponent(GameObject entityRoot, PlayerCinematicController cinematicController)
    {
        // Try entity root first
        PrecursorDisableGunTerminal terminal = entityRoot.GetComponent<PrecursorDisableGunTerminal>();
        if (terminal) return terminal;

        // Try children of entity root
        terminal = entityRoot.GetComponentInChildren<PrecursorDisableGunTerminal>();
        if (terminal) return terminal;

        // Try parent hierarchy from controller
        terminal = cinematicController.GetComponentInParent<PrecursorDisableGunTerminal>();
        if (terminal) return terminal;

        Log.Warn($"[{nameof(PlayerCinematicController_StartCinematicMode_Patch)}] Could not find PrecursorDisableGunTerminal component for gun terminal cinematic");
        return null;
    }

    private readonly struct CinematicInteraction(PlayerCinematicController controller, NitroxEntity entity, MultiplayerCinematicController multiplayerController) : LockRequestContext
    {
        public PlayerCinematicController Controller { get; } = controller;
        public NitroxEntity Entity { get; } = entity;
        public MultiplayerCinematicController MultiplayerController { get; } = multiplayerController;
    }
}
