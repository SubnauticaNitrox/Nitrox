using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class BaseNuclearReactor_Update_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseNuclearReactor t) => t.Update());

	public static void Postfix(BaseNuclearReactor __instance)
	{
		BasePowerBroadcaster.BroadcastIfOwner(__instance, __instance._powerSource, NitroxPatch.Resolve<SimulationOwnership>(), NitroxPatch.Resolve<Entities>());
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, null, new Action<BaseNuclearReactor>(Postfix).Method);
	}
}


