using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class PlayerCinematicController_OnPlayerCinematicModeEnd_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.OnPlayerCinematicModeEnd());

	public static void Prefix(PlayerCinematicController __instance)
	{
		if (__instance.cinematicModeActive && __instance.TryGetComponentInParent<NitroxEntity>(out var component, includeInactive: true) && !component.gameObject.GetComponent<Bed>() && NitroxPatch.Resolve<LocalPlayer>().SessionId.HasValue)
		{
			int hashCode = __instance.gameObject.GetHierarchyPath(component.gameObject).GetHashCode();
			NitroxPatch.Resolve<PlayerCinematics>().EndCinematicMode(NitroxPatch.Resolve<LocalPlayer>().SessionId.Value, component.Id, hashCode, __instance.playerViewAnimationName);
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, targetMethod, new Action<PlayerCinematicController>(Prefix).Method);
	}
}
