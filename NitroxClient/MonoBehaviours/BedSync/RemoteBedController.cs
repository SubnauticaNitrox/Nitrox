using System.Collections.Generic;
using Nitrox.Model.Logger;
using NitroxClient.GameLogic;
using UWE;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.BedSync;

public class RemoteBedController : MonoBehaviour
{
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

	private Bed bed;

	private Dictionary<RemotePlayer, PlayerAnimationState> playerStates = new Dictionary<RemotePlayer, PlayerAnimationState>();

	private void Start()
	{
		bed = GetComponent<Bed>();
		if (!bed)
		{
			Log.Error("[BedSync] No Bed component found on " + base.gameObject.name);
			Object.Destroy(this);
		}
	}

	public void StartBedAnimation(RemotePlayer player, string animationKey)
	{
		if (!bed || player == null || !player.Body || player.AnimationController == null)
		{
			return;
		}
		if (playerStates.ContainsKey(player))
		{
			CleanupPlayerAnimation(player);
		}
		Vector3 animPosition;
		PlayerCinematicController cinematicController = GetCinematicController(animationKey, out animPosition);
		if (cinematicController == null)
		{
			return;
		}
		AnimationClip animationClip = FindAnimationClip(cinematicController.animParam);
		if (!(animationClip == null))
		{
			PlayerAnimationState value = new PlayerAnimationState
			{
				Player = player,
				AnimationKey = animationKey,
				CinematicController = cinematicController,
				AnimationClip = animationClip,
				AnimationStartTime = Time.time,
				AnimationDuration = animationClip.length,
				StartPosition = bed.animator.transform.TransformPoint(animPosition),
				StartRotation = bed.animator.transform.rotation,
				FrameCount = 0
			};
			playerStates[player] = value;
			player.AnimationController["cinematics_enabled"] = true;
			player.AnimationController[cinematicController.playerViewAnimationName] = true;
			if ((bool)player.RigidBody)
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(player.RigidBody, isKinematic: true);
			}
			player.AnimationController.UpdatePlayerAnimations = false;
			player.InCinematic = true;
		}
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
		List<RemotePlayer> list = new List<RemotePlayer>();
		foreach (KeyValuePair<RemotePlayer, PlayerAnimationState> playerState in playerStates)
		{
			RemotePlayer key = playerState.Key;
			PlayerAnimationState value = playerState.Value;
			if (key == null || !key.Body || value.AnimationClip == null)
			{
				list.Add(key);
				continue;
			}
			value.FrameCount++;
			float num = Mathf.Clamp01((Time.time - value.AnimationStartTime) / value.AnimationDuration);
			float time = num * value.AnimationDuration;
			Transform animatedTransform = value.CinematicController.animatedTransform;
			Vector3 localPosition = animatedTransform.localPosition;
			Quaternion localRotation = animatedTransform.localRotation;
			value.AnimationClip.SampleAnimation(bed.animator.gameObject, time);
			Vector3 position = animatedTransform.position;
			Quaternion rotation = animatedTransform.rotation;
			animatedTransform.localPosition = localPosition;
			animatedTransform.localRotation = localRotation;
			Transform obj = key.Body.transform;
			obj.position = position;
			obj.rotation = rotation;
			if (value.AnimationKey != null && value.AnimationKey.StartsWith("bed_up"))
			{
				if (num >= 1f)
				{
					list.Add(key);
				}
				else if (Time.time - value.AnimationStartTime > value.AnimationDuration + 1f)
				{
					list.Add(key);
				}
			}
		}
		foreach (RemotePlayer item in list)
		{
			CleanupPlayerAnimation(item);
		}
	}

	private AnimationClip FindAnimationClip(string animParam)
	{
		RuntimeAnimatorController runtimeAnimatorController = bed.animator.runtimeAnimatorController;
		if (runtimeAnimatorController == null)
		{
			return null;
		}
		AnimationClip[] animationClips = runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (animationClip.name.ToLower().Contains(animParam.ToLower()) || animParam.ToLower().Contains(animationClip.name.ToLower()))
			{
				return animationClip;
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
		if (playerStates.TryGetValue(player, out PlayerAnimationState value))
		{
			if (player.AnimationController != null && value.CinematicController != null)
			{
				player.AnimationController[value.CinematicController.playerViewAnimationName] = false;
				player.AnimationController["cinematics_enabled"] = false;
				player.AnimationController.UpdatePlayerAnimations = true;
			}
			if ((bool)player.RigidBody)
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(player.RigidBody, isKinematic: false, setCollisionDetectionMode: true);
			}
			player.InCinematic = false;
			playerStates.Remove(player);
		}
	}

	private void OnDestroy()
	{
		foreach (PlayerAnimationState value in playerStates.Values)
		{
			if (value.Player != null)
			{
				value.Player.InCinematic = false;
				if (value.Player.AnimationController != null)
				{
					value.Player.AnimationController.UpdatePlayerAnimations = true;
				}
			}
		}
		playerStates.Clear();
	}
}


