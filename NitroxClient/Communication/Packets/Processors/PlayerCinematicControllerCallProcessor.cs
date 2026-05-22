using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using Story;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerCinematicControllerCallProcessor(PlayerManager playerManager) : IClientPacketProcessor<PlayerCinematicControllerCall>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, PlayerCinematicControllerCall packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.ControllerID, out GameObject entity))
        {
            return Task.CompletedTask;
        }

        if (!entity.TryGetComponent(out MultiplayerCinematicReference reference))
        {
            Log.Warn($"Couldn't find {nameof(MultiplayerCinematicReference)} on {entity.name}:{packet.ControllerID}");
            return Task.CompletedTask;
        }

        if (!playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer))
        {
            Log.Warn($"Couldn't find remote player {packet.SessionId} for cinematic");
            return Task.CompletedTask;
        }

        if (packet.StartPlaying)
        {
            // For gun terminal, apply animation parameters before starting cinematic
            ApplyGunTerminalAnimationParameters(entity, remotePlayer);
            
            // Set InCinematic flag to prevent movement packets from overriding animation state
            remotePlayer.InCinematic = true;
            remotePlayer.AnimationController.UpdatePlayerAnimations = false;
            reference.CallStartCinematicMode(packet.Key, packet.ControllerNameHash, remotePlayer);
        }
        else
        {
            reference.CallCinematicModeEnd(packet.Key, packet.ControllerNameHash, remotePlayer);
            // Clear InCinematic flag to allow movement packets to control animations again
            remotePlayer.InCinematic = false;
            remotePlayer.AnimationController.UpdatePlayerAnimations = true;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies gun terminal-specific animation parameters to the cinematic animator and remote player.
    /// Must be called BEFORE starting the cinematic to ensure correct animations play.
    /// </summary>
    private static void ApplyGunTerminalAnimationParameters(GameObject entity, RemotePlayer remotePlayer)
    {
        // Check if this is a gun terminal cinematic
        if (!entity.TryGetComponent(out PrecursorDisableGunTerminal terminal))
        {
            return;
        }

        // Find the cinematic controller (should be on the terminal)
        if (!terminal.TryGetComponent(out PlayerCinematicController cinematicController))
        {
            Log.Warn($"[GunTerminal] No PlayerCinematicController found on gun terminal");
            return;
        }

        // Check if player is cured (story goal)
        bool playerCured = false;
        if (StoryGoalManager.main != null && terminal.onPlayerCuredGoal != null)
        {
            playerCured = StoryGoalManager.main.IsGoalComplete(terminal.onPlayerCuredGoal.key);
        }

        // Apply parameters to the cinematic animator (gun terminal's animator)
        SafeAnimator.SetBool(cinematicController.animator, "first_use", terminal.firstUse);
        SafeAnimator.SetBool(cinematicController.animator, "cured", playerCured);

        // Apply parameters to the remote player's animator
        Animator playerAnimator = remotePlayer.Body.GetComponentInChildren<Animator>();
        if (playerAnimator != null && playerAnimator.gameObject.activeInHierarchy)
        {
            SafeAnimator.SetBool(playerAnimator, "using_tool_first", terminal.firstUse);
            SafeAnimator.SetBool(playerAnimator, "cured", playerCured);
        }

        Log.Debug($"[GunTerminal] Applied animation parameters - firstUse: {terminal.firstUse}, cured: {playerCured}");
    }
}
