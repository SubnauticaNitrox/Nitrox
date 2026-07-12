using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using Story;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class PlayerCinematicController_StartCinematicMode_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.StartCinematicMode(null));

	public static void Prefix(PlayerCinematicController __instance)
	{
		if (!__instance.cinematicModeActive && (bool)Player.main && Player.main.gameObject.activeInHierarchy && __instance.TryGetComponent<MultiplayerCinematicController>(out var component) && !__instance.GetComponentInParent<Bed>() && __instance.TryGetComponentInParent<NitroxEntity>(out var component2, includeInactive: true) && NitroxPatch.Resolve<LocalPlayer>().SessionId.HasValue)
		{
			component.CallAllCinematicModeEnd();
			int hashCode = __instance.gameObject.GetHierarchyPath(component2.gameObject).GetHashCode();
			Dictionary<string, bool> animationParameters = CaptureAnimationParameters(__instance, component2.gameObject);
			NitroxPatch.Resolve<PlayerCinematics>().StartCinematicMode(NitroxPatch.Resolve<LocalPlayer>().SessionId.Value, component2.Id, hashCode, __instance.playerViewAnimationName, animationParameters);
		}
	}

	private static Dictionary<string, bool> CaptureAnimationParameters(PlayerCinematicController cinematicController, GameObject entityRoot)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		if (cinematicController.playerViewAnimationName == "precursor_deactivate_gun")
		{
			PrecursorDisableGunTerminal precursorDisableGunTerminal = FindTerminalComponent(entityRoot, cinematicController);
			if ((bool)precursorDisableGunTerminal)
			{
				bool value = StoryGoalManager.main != null && precursorDisableGunTerminal.onPlayerCuredGoal != null && StoryGoalManager.main.IsGoalComplete(precursorDisableGunTerminal.onPlayerCuredGoal.key);
				dictionary["first_use"] = precursorDisableGunTerminal.firstUse;
				dictionary["cured"] = value;
				dictionary["using_tool_first"] = precursorDisableGunTerminal.firstUse;
			}
		}
		return dictionary;
	}

	private static PrecursorDisableGunTerminal FindTerminalComponent(GameObject entityRoot, PlayerCinematicController cinematicController)
	{
		PrecursorDisableGunTerminal component = entityRoot.GetComponent<PrecursorDisableGunTerminal>();
		if ((bool)component)
		{
			return component;
		}
		component = entityRoot.GetComponentInChildren<PrecursorDisableGunTerminal>();
		if ((bool)component)
		{
			return component;
		}
		component = cinematicController.GetComponentInParent<PrecursorDisableGunTerminal>();
		if ((bool)component)
		{
			return component;
		}
		Log.Warn("Could not find PrecursorDisableGunTerminal component for gun terminal cinematic");
		return null;
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, targetMethod, new Action<PlayerCinematicController>(Prefix).Method);
	}
}
