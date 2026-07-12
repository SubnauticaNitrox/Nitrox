using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class Welder_Update_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Welder t) => t.Update());

	internal static bool localWasWelding;

	public static bool Prefix(Welder __instance)
	{
		if ((bool)Player.main)
		{
			return ((PlayerTool)__instance).usingPlayer == Player.main;
		}
		return false;
	}

	public static void Postfix(Welder __instance)
	{
		if ((bool)Player.main && !(((PlayerTool)__instance).usingPlayer != Player.main) && __instance.usedThisFrame != localWasWelding)
		{
			localWasWelding = __instance.usedThisFrame;
			NitroxPatch.Resolve<LocalPlayer>().BroadcastWelderUse(__instance.usedThisFrame);
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, new Func<Welder, bool>(Prefix).Method, new Action<Welder>(Postfix).Method);
	}
}


