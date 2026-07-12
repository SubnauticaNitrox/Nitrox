using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class Charger_OnEquip_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Charger t) => t.OnEquip((string)null, (InventoryItem)null));

	public static void Postfix(Charger __instance)
	{
		if (!__instance.opened)
		{
			__instance.opened = true;
			__instance.animator.SetBool(__instance.animParamOpen, value: true);
			__instance.ToggleUI(active: true);
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, null, new Action<Charger>(Postfix).Method);
	}
}


