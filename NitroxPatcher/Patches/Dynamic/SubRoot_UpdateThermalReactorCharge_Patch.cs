using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class SubRoot_UpdateThermalReactorCharge_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubRoot t) => t.UpdateThermalReactorCharge());

	public static bool Prefix(SubRoot __instance)
	{
		if (__instance.TryGetNitroxId(out NitroxId nitroxId) && !NitroxPatch.Resolve<SimulationOwnership>().HasAnyLockType(nitroxId))
		{
			return false;
		}
		return true;
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, new Func<SubRoot, bool>(Prefix).Method);
	}
}


