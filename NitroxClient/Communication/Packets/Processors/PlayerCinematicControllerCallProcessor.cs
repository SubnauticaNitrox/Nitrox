using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.BedSync;
using NitroxClient.MonoBehaviours.CinematicController;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerCinematicControllerCallProcessor(PlayerManager playerManager) : IClientPacketProcessor<PlayerCinematicControllerCall>, IClientPacketProcessor, IPacketProcessor, IPacketProcessor<ClientProcessorContext, PlayerCinematicControllerCall>
{
	private readonly PlayerManager playerManager = playerManager;

	public Task Process(ClientProcessorContext context, PlayerCinematicControllerCall packet)
	{
		if (!NitroxEntity.TryGetObjectFrom(packet.ControllerID, out GameObject gameObject))
		{
			Log.Warn($"Could not find entity with ID {packet.ControllerID} for cinematic");
			return Task.CompletedTask;
		}
		if (gameObject.TryGetComponent<RemoteBedController>(out var component))
		{
			if (!playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer))
			{
				return Task.CompletedTask;
			}
			if (!remotePlayer.Body || !remotePlayer.Body.activeInHierarchy)
			{
				return Task.CompletedTask;
			}
			if (packet.StartPlaying)
			{
				remotePlayer.InCinematic = true;
				if ((bool)remotePlayer.AnimationController)
				{
					remotePlayer.AnimationController.UpdatePlayerAnimations = false;
				}
				component.StartBedAnimation(remotePlayer, packet.Key);
			}
			else
			{
				component.EndBedAnimation(remotePlayer, packet.Key);
				remotePlayer.InCinematic = false;
				if ((bool)remotePlayer.AnimationController)
				{
					remotePlayer.AnimationController.UpdatePlayerAnimations = true;
				}
			}
			return Task.CompletedTask;
		}
		if (!gameObject.TryGetComponent<MultiplayerCinematicReference>(out var component2))
		{
			return Task.CompletedTask;
		}
		if (!playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer2))
		{
			return Task.CompletedTask;
		}
		if (!remotePlayer2.Body || !remotePlayer2.Body.activeInHierarchy || !remotePlayer2.AnimationController)
		{
			return Task.CompletedTask;
		}
		if (packet.StartPlaying)
		{
			if (packet.AnimationParameters != null && packet.AnimationParameters.Count > 0)
			{
				ApplyAnimationParameters(component2, packet.Key, packet.ControllerNameHash, remotePlayer2, packet.AnimationParameters);
			}
			remotePlayer2.InCinematic = true;
			remotePlayer2.AnimationController.UpdatePlayerAnimations = false;
			component2.CallStartCinematicMode(packet.Key, packet.ControllerNameHash, remotePlayer2);
		}
		else
		{
			component2.CallCinematicModeEnd(packet.Key, packet.ControllerNameHash, remotePlayer2);
			remotePlayer2.InCinematic = false;
			remotePlayer2.AnimationController.UpdatePlayerAnimations = true;
		}
		return Task.CompletedTask;
	}

	private static void ApplyAnimationParameters(MultiplayerCinematicReference reference, string key, int controllerNameHash, RemotePlayer remotePlayer, Dictionary<string, bool> animationParameters)
	{
		if (!TryGetCinematicController(reference, key, controllerNameHash, out PlayerCinematicController cinematicController))
		{
			return;
		}
		if (cinematicController.animator != null)
		{
			foreach (KeyValuePair<string, bool> animationParameter in animationParameters)
			{
				if (animationParameter.Key == "first_use" || animationParameter.Key == "cured")
				{
					SafeAnimator.SetBool(cinematicController.animator, animationParameter.Key, animationParameter.Value);
				}
			}
		}
		Animator componentInChildren = remotePlayer.Body.GetComponentInChildren<Animator>();
		if (!componentInChildren || !componentInChildren.gameObject.activeInHierarchy)
		{
			return;
		}
		foreach (KeyValuePair<string, bool> animationParameter2 in animationParameters)
		{
			if (animationParameter2.Key == "using_tool_first" || animationParameter2.Key == "cured")
			{
				SafeAnimator.SetBool(componentInChildren, animationParameter2.Key, animationParameter2.Value);
			}
		}
	}

	private static bool TryGetCinematicController(MultiplayerCinematicReference reference, string key, int controllerNameHash, out PlayerCinematicController cinematicController)
	{
		cinematicController = null;
		FieldInfo field = typeof(MultiplayerCinematicReference).GetField("controllerByKey", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
		{
			return false;
		}
		if (!(field.GetValue(reference) is Dictionary<string, Dictionary<int, MultiplayerCinematicController>> dictionary) || !dictionary.TryGetValue(key, out var value) || !value.TryGetValue(controllerNameHash, out var value2))
		{
			return false;
		}
		FieldInfo field2 = typeof(MultiplayerCinematicController).GetField("playerController", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field2 == null)
		{
			return false;
		}
		cinematicController = field2.GetValue(value2) as PlayerCinematicController;
		return cinematicController != null;
	}
}
