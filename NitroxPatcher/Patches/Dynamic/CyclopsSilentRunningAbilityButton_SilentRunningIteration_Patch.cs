using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class CyclopsSilentRunningAbilityButton_SilentRunningIteration_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSilentRunningAbilityButton t) => t.SilentRunningIteration());

	public static bool Prefix(CyclopsSilentRunningAbilityButton __instance)
	{
		if (__instance.subRoot.TryGetNitroxId(out NitroxId nitroxId))
		{
			return NitroxPatch.Resolve<SimulationOwnership>().HasAnyLockType(nitroxId);
		}
		return false;
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, new Func<CyclopsSilentRunningAbilityButton, bool>(Prefix).Method);
	}
}


