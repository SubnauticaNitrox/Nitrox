using NitroxClient.GameLogic;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.BedSync;

/// <summary>
/// Handles bed animation sync for remote players.
/// Uses manual animation sampling to avoid conflicts with the shared bed animator.
/// </summary>
public class RemoteBedController : MonoBehaviour
{
    private Bed bed;
    private Dictionary<RemotePlayer, PlayerAnimationState> playerStates = new Dictionary<RemotePlayer, PlayerAnimationState>();

    private class PlayerAnimationState
    {
        public RemotePlayer Player;
        public string AnimationKey;
        public PlayerCinematicController CinematicController;
        public AnimationClip AnimationClip;
        public float AnimationStartTime;
        public float AnimationDuration;
        public Vector3 StartPosition;
        public Quaternion StartRotation;
        public int FrameCount;
    }

    private void Start()
    {
        bed = GetComponent<Bed>();
        if (!bed)
        {
            Log.Error($"[BedSync] No Bed component found on {gameObject.name}");
            Destroy(this);
        }
    }

    public void StartBedAnimation(RemotePlayer player, string animationKey)
    {
        if (!bed || player == null || !player.Body || player.AnimationController == null)
        {
            return;
        }

        // End previous animation for this player if any
        if (playerStates.ContainsKey(player))
        {
            CleanupPlayerAnimation(player);
        }

        // Get the cinematic controller
        PlayerCinematicController cinematicController = GetCinematicController(animationKey, out Vector3 animPosition);
        if (cinematicController == null)
        {
            return;
        }

        // Find the animation clip for this cinematic
        AnimationClip animClip = FindAnimationClip(cinematicController.animParam);
        if (animClip == null)
        {
            return;
        }

        // Create state for this player
        PlayerAnimationState state = new PlayerAnimationState
        {
            Player = player,
            AnimationKey = animationKey,
            CinematicController = cinematicController,
            AnimationClip = animClip,
            AnimationStartTime = Time.time,
            AnimationDuration = animClip.length,
            StartPosition = bed.animator.transform.TransformPoint(animPosition),
            StartRotation = bed.animator.transform.rotation,
            FrameCount = 0
        };
        playerStates[player] = state;

        // Set player animator parameters
        player.AnimationController["cinematics_enabled"] = true;
        player.AnimationController[cinematicController.playerViewAnimationName] = true;

        // Make rigidbody kinematic
        if (player.RigidBody)
        {
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(player.RigidBody, true, false);
        }

        // Set flags
        player.AnimationController.UpdatePlayerAnimations = false;
        player.InCinematic = true;
    }

    public void EndBedAnimation(RemotePlayer player, string animationKey)
    {
        if (playerStates.ContainsKey(player))
        {
            CleanupPlayerAnimation(player);
        }
    }

    private void LateUpdate()
    {
        List<RemotePlayer> playersToRemove = new List<RemotePlayer>();

        foreach (var kvp in playerStates)
        {
            RemotePlayer player = kvp.Key;
            PlayerAnimationState state = kvp.Value;

            if (player == null || !player.Body || state.AnimationClip == null)
            {
                playersToRemove.Add(player);
                continue;
            }

            state.FrameCount++;

            // Calculate animation time
            float elapsedTime = Time.time - state.AnimationStartTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / state.AnimationDuration);
            float sampleTime = normalizedTime * state.AnimationDuration;

            // Sample the animation clip to get the transform at this time
            // We need to sample on the bed's animatedTransform to get the correct position
            Transform animatedTransform = state.CinematicController.animatedTransform;
            
            // Store original transform values
            Vector3 originalLocalPos = animatedTransform.localPosition;
            Quaternion originalLocalRot = animatedTransform.localRotation;

            // Sample the animation
            state.AnimationClip.SampleAnimation(bed.animator.gameObject, sampleTime);

            // Get the sampled position/rotation
            Vector3 sampledWorldPos = animatedTransform.position;
            Quaternion sampledWorldRot = animatedTransform.rotation;

            // Restore original transform (so we don't affect the bed visually)
            animatedTransform.localPosition = originalLocalPos;
            animatedTransform.localRotation = originalLocalRot;

            // Apply to player
            Transform playerTransform = player.Body.transform;
            playerTransform.position = sampledWorldPos;
            playerTransform.rotation = sampledWorldRot;

            // Auto-detect when stand-up animation finishes
            if (state.AnimationKey != null && state.AnimationKey.StartsWith("bed_up"))
            {
                // Animation is complete when normalizedTime >= 1.0
                if (normalizedTime >= 1.0f)
                {
                    playersToRemove.Add(player);
                    continue;
                }
                
                // Fallback: if animation has been running for longer than its duration + 1 second, force end it
                float animElapsedTime = Time.time - state.AnimationStartTime;
                if (animElapsedTime > state.AnimationDuration + 1.0f)
                {
                    playersToRemove.Add(player);
                    continue;
                }
            }
        }

        // Clean up finished animations
        foreach (RemotePlayer player in playersToRemove)
        {
            CleanupPlayerAnimation(player);
        }
    }

    private AnimationClip FindAnimationClip(string animParam)
    {
        RuntimeAnimatorController controller = bed.animator.runtimeAnimatorController;
        if (controller == null)
        {
            return null;
        }

        AnimationClip[] clips = controller.animationClips;
        
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.ToLower().Contains(animParam.ToLower()) || 
                animParam.ToLower().Contains(clip.name.ToLower()))
            {
                return clip;
            }
        }

        return null;
    }

    private PlayerCinematicController GetCinematicController(string animationKey, out Vector3 animPosition)
    {
        animPosition = Vector3.zero;
        
        switch (animationKey)
        {
            case "bed_down_left":
                animPosition = bed.leftAnimPosition;
                return bed.leftLieDownCinematicController;
            case "bed_down_right":
                animPosition = bed.rightAnimPosition;
                return bed.rightLieDownCinematicController;
            case "bed_up_left":
                animPosition = bed.leftAnimPosition;
                return bed.leftStandUpCinematicController;
            case "bed_up_right":
                animPosition = bed.rightAnimPosition;
                return bed.rightStandUpCinematicController;
            default:
                return null;
        }
    }

    private void CleanupPlayerAnimation(RemotePlayer player)
    {
        if (!playerStates.TryGetValue(player, out PlayerAnimationState state))
        {
            return;
        }

        // Reset player animator
        if (player.AnimationController != null && state.CinematicController != null)
        {
            player.AnimationController[state.CinematicController.playerViewAnimationName] = false;
            player.AnimationController["cinematics_enabled"] = false;
            player.AnimationController.UpdatePlayerAnimations = true;
        }

        // Restore rigidbody
        if (player.RigidBody)
        {
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(player.RigidBody, false, true);
        }

        // Clear flags
        player.InCinematic = false;

        // Remove state
        playerStates.Remove(player);
    }

    private void OnDestroy()
    {
        // Clean up all player states
        foreach (var state in playerStates.Values)
        {
            if (state.Player != null)
            {
                state.Player.InCinematic = false;
                if (state.Player.AnimationController != null)
                {
                    state.Player.AnimationController.UpdatePlayerAnimations = true;
                }
            }
        }
        playerStates.Clear();
    }
}
