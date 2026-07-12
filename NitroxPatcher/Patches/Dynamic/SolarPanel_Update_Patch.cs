using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class SolarPanel_Update_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SolarPanel t) => t.Update());

	public static bool Prefix(SolarPanel __instance)
	{
		if (__instance.TryGetNitroxId(out NitroxId nitroxId))
		{
			return NitroxPatch.Resolve<SimulationOwnership>().HasAnyLockType(nitroxId);
		}
		return true;
	}

	public static void Postfix(SolarPanel __instance)
	{
		BasePowerBroadcaster.BroadcastIfOwner(__instance, __instance.powerSource, NitroxPatch.Resolve<SimulationOwnership>(), NitroxPatch.Resolve<Entities>());
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, new Func<SolarPanel, bool>(Prefix).Method, new Action<SolarPanel>(Postfix).Method);
	}
}


