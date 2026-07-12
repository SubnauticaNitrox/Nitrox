using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class Welder_OnHolster_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Welder t) => t.OnHolster());

	public static void Postfix()
	{
		if (Welder_Update_Patch.localWasWelding)
		{
			Welder_Update_Patch.localWasWelding = false;
			if ((bool)Player.main)
			{
				NitroxPatch.Resolve<LocalPlayer>().BroadcastWelderUse(welding: false);
			}
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, null, new Action(Postfix).Method);
	}
}


