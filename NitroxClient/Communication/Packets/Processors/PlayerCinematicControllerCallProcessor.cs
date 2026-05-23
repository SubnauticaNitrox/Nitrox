using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerCinematicControllerCallProcessor(PlayerManager playerManager) : IClientPacketProcessor<PlayerCinematicControllerCall>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, PlayerCinematicControllerCall packet)
    {
        Log.Info($"[CinematicLock] Received cinematic packet:");
        Log.Info($"  - Player: {packet.SessionId}");
        Log.Info($"  - Controller ID: {packet.ControllerID}");
        Log.Info($"  - Key: {packet.Key}");
        Log.Info($"  - Starting: {packet.StartPlaying}");

        if (!NitroxEntity.TryGetObjectFrom(packet.ControllerID, out GameObject entity))
        {
            Log.Warn($"[CinematicLock] Could not find entity with ID {packet.ControllerID} - entity may not be loaded yet");
            return Task.CompletedTask;
        }

        Log.Info($"  - Entity GameObject: {entity.name}");

        if (!entity.TryGetComponent(out MultiplayerCinematicReference reference))
        {
            Log.Warn($"[CinematicLock] No MultiplayerCinematicReference on {entity.name}");
            return Task.CompletedTask;
        }

        if (!playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer))
        {
            Log.Warn($"[CinematicLock] Could not find remote player {packet.SessionId}");
            return Task.CompletedTask;
        }

        // Defensive check: Ensure remote player is fully initialized before processing cinematic packets
        // This prevents race conditions when a player joins mid-cinematic
        if (!remotePlayer.Body || !remotePlayer.Body.activeInHierarchy)
        {
            Log.Warn($"[CinematicLock] Remote player {remotePlayer.PlayerName} body not ready (Body: {remotePlayer.Body != null}, Active: {(remotePlayer.Body ? remotePlayer.Body.activeInHierarchy : false)}) - ignoring cinematic packet to prevent race condition");
            return Task.CompletedTask;
        }

        if (!remotePlayer.AnimationController)
        {
            Log.Warn($"[CinematicLock] Remote player {remotePlayer.PlayerName} AnimationController not ready - ignoring cinematic packet to prevent race condition");
            return Task.CompletedTask;
        }

        Log.Info($"  - Remote Player: {remotePlayer.PlayerName}");

        if (packet.StartPlaying)
        {
            // Apply animation parameters before starting cinematic
            if (packet.AnimationParameters != null && packet.AnimationParameters.Count > 0)
            {
                Log.Info($"[CinematicLock] Applying {packet.AnimationParameters.Count} animation parameters");
                ApplyAnimationParameters(reference, packet.Key, packet.ControllerNameHash, remotePlayer, packet.AnimationParameters);
            }
            
            // Set InCinematic flag to prevent movement packets from overriding animation state
            remotePlayer.InCinematic = true;
            remotePlayer.AnimationController.UpdatePlayerAnimations = false;
            Log.Info($"[CinematicLock] Starting cinematic for remote player {remotePlayer.PlayerName}");
            reference.CallStartCinematicMode(packet.Key, packet.ControllerNameHash, remotePlayer);
        }
        else
        {
            Log.Info($"[CinematicLock] Ending cinematic for remote player {remotePlayer.PlayerName}");
            reference.CallCinematicModeEnd(packet.Key, packet.ControllerNameHash, remotePlayer);
            // Clear InCinematic flag to allow movement packets to control animations again
            remotePlayer.InCinematic = false;
            remotePlayer.AnimationController.UpdatePlayerAnimations = true;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies animation parameters to the cinematic and player animators.
    /// This ensures animations sync correctly when metadata timing is insufficient.
    /// </summary>
    private static void ApplyAnimationParameters(MultiplayerCinematicReference reference, string key, int controllerNameHash, RemotePlayer remotePlayer, Dictionary<string, bool> animationParameters)
    {
        // Find the specific cinematic controller
        if (!TryGetCinematicController(reference, key, controllerNameHash, out PlayerCinematicController cinematicController))
        {
            Log.Warn($"[{nameof(PlayerCinematicControllerCallProcessor)}] Could not find cinematic controller for key '{key}' and hash {controllerNameHash}");
            return;
        }

        // Apply parameters to the cinematic animator (e.g., gun terminal's animator)
        if (cinematicController.animator != null)
        {
            foreach (var param in animationParameters)
            {
                // Terminal-specific parameters
                if (param.Key == "first_use" || param.Key == "cured")
                {
                    SafeAnimator.SetBool(cinematicController.animator, param.Key, param.Value);
                }
            }
        }

        // Apply parameters to the remote player's animator
        Animator playerAnimator = remotePlayer.Body.GetComponentInChildren<Animator>();
        if (playerAnimator && playerAnimator.gameObject.activeInHierarchy)
        {
            foreach (var param in animationParameters)
            {
                // Player-specific parameters
                if (param.Key == "using_tool_first" || param.Key == "cured")
                {
                    SafeAnimator.SetBool(playerAnimator, param.Key, param.Value);
                }
            }
        }
    }

    /// <summary>
    /// Gets the PlayerCinematicController from the MultiplayerCinematicReference using reflection.
    /// </summary>
    private static bool TryGetCinematicController(MultiplayerCinematicReference reference, string key, int controllerNameHash, out PlayerCinematicController cinematicController)
    {
        cinematicController = null;

        // Access private controllerByKey field
        var controllerByKeyField = typeof(MultiplayerCinematicReference).GetField("controllerByKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (controllerByKeyField == null)
        {
            return false;
        }

        var controllerByKey = controllerByKeyField.GetValue(reference) as Dictionary<string, Dictionary<int, MultiplayerCinematicController>>;
        if (controllerByKey == null || !controllerByKey.TryGetValue(key, out var controllers) || !controllers.TryGetValue(controllerNameHash, out var multiplayerController))
        {
            return false;
        }

        // Access private playerController field
        var playerControllerField = typeof(MultiplayerCinematicController).GetField("playerController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (playerControllerField == null)
        {
            return false;
        }

        cinematicController = playerControllerField.GetValue(multiplayerController) as PlayerCinematicController;
        return cinematicController != null;
    }
}
