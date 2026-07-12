using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class FireExtinguisher_Update_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FireExtinguisher t) => t.Update());

	private static bool localWasSpraying;

	public static bool Prefix(FireExtinguisher __instance)
	{
		if ((bool)Player.main)
		{
			return ((PlayerTool)__instance).usingPlayer == Player.main;
		}
		return false;
	}

	public static void Postfix(FireExtinguisher __instance)
	{
		if ((bool)Player.main && !(((PlayerTool)__instance).usingPlayer != Player.main) && __instance.fxIsPlaying != localWasSpraying)
		{
			localWasSpraying = __instance.fxIsPlaying;
			NitroxPatch.Resolve<LocalPlayer>().BroadcastFireExtinguisherUse(__instance.fxIsPlaying);
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, new Func<FireExtinguisher, bool>(Prefix).Method, new Action<FireExtinguisher>(Postfix).Method);
	}
}


