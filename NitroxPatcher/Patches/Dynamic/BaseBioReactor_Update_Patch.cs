using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class BaseBioReactor_Update_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseBioReactor t) => t.Update());

	public static void Postfix(BaseBioReactor __instance)
	{
		BasePowerBroadcaster.BroadcastIfOwner(__instance, __instance._powerSource, NitroxPatch.Resolve<SimulationOwnership>(), NitroxPatch.Resolve<Entities>());
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, null, new Action<BaseBioReactor>(Postfix).Method);
	}
}


