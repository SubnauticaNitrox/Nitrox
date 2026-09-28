using System.Collections.Generic;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.CinematicController;

/// <summary>
/// Bed's rig is shared by both sides and only repositioned locally by vanilla <see cref="Bed.OnHandClick"/>, so a remote client's
/// live <see cref="PlayerCinematicController.animatedTransform"/> is wrong (or, with two occupants, right for only one of the sides)
/// Samples the clip directly instead, making it never disturb the shared rig for anyone else.
/// </summary>
internal class BedRemotePositioner : MonoBehaviour
{
    private readonly Dictionary<RemotePlayer, State> stateByPlayer = new();
    private Bed bed;

    private readonly struct State(PlayerCinematicController controller, AnimationClip clip, float startTime)
    {
        public readonly PlayerCinematicController Controller = controller;
        public readonly AnimationClip Clip = clip;
        public readonly float StartTime = startTime;
    }

    private void Awake()
    {
        bed = GetComponent<Bed>();
    }

    public void Begin(RemotePlayer player, PlayerCinematicController controller)
    {
        if (!controller.animatedTransform || !TryFindClip(controller.animParam, out AnimationClip clip))
        {
            return;
        }

        stateByPlayer[player] = new State(controller, clip, Time.time);
    }

    public void End(RemotePlayer player)
    {
        stateByPlayer.Remove(player);
    }

    private bool TryFindClip(string animParam, out AnimationClip clip)
    {
        clip = null;
        if (!bed.animator.runtimeAnimatorController)
        {
            return false;
        }

        foreach (AnimationClip candidate in bed.animator.runtimeAnimatorController.animationClips)
        {
            if (candidate.name.ToLower().Contains(animParam.ToLower()))
            {
                clip = candidate;
                return true;
            }
        }

        return false;
    }

    private void LateUpdate()
    {
        if (stateByPlayer.Count == 0)
        {
            return;
        }

        List<RemotePlayer> toRemove = null;
        foreach (KeyValuePair<RemotePlayer, State> entry in stateByPlayer)
        {
            if (!entry.Key.Body || entry.Key.InCinematicEntityId == null)
            {
                (toRemove ??= new List<RemotePlayer>()).Add(entry.Key);
                continue;
            }

            ApplySampledPosition(entry.Key, entry.Value);
        }

        if (toRemove != null)
        {
            foreach (RemotePlayer player in toRemove)
            {
                stateByPlayer.Remove(player);
            }
        }
    }

    private void ApplySampledPosition(RemotePlayer player, State state)
    {
        Transform animatedTransform = state.Controller.animatedTransform;
        Vector3 originalLocalPosition = animatedTransform.localPosition;
        Quaternion originalLocalRotation = animatedTransform.localRotation;

        float sampleTime = Mathf.Min(Time.time - state.StartTime, state.Clip.length);
        state.Clip.SampleAnimation(bed.animator.gameObject, sampleTime);
        Vector3 sampledPosition = animatedTransform.position;
        Quaternion sampledRotation = animatedTransform.rotation;

        animatedTransform.localPosition = originalLocalPosition;
        animatedTransform.localRotation = originalLocalRotation;

        player.Body.transform.SetPositionAndRotation(sampledPosition, sampledRotation);
    }
}
