using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class BaseBioReactor_ProducePower_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private delegate void PrefixDelegate(BaseBioReactor instance, out List<NitroxId> state);
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseBioReactor t) => t.ProducePower(0f));

	public static void Prefix(BaseBioReactor __instance, out List<NitroxId> __state)
	{
		__state = CollectItemIds(__instance);
	}

	public static void Postfix(BaseBioReactor __instance, List<NitroxId> __state)
	{
		if (__state.Count == 0 || !__instance.TryGetNitroxId(out NitroxId nitroxId) || !NitroxPatch.Resolve<SimulationOwnership>().HasAnyLockType(nitroxId))
		{
			return;
		}
		HashSet<NitroxId> hashSet = new HashSet<NitroxId>(CollectItemIds(__instance));
		foreach (NitroxId item in __state)
		{
			if (!hashSet.Contains(item))
			{
				NitroxPatch.Resolve<Entities>().RemoveEntity(item);
				NitroxPatch.Resolve<IPacketSender>().Send(new EntityDestroyed(item));
			}
		}
	}

	private static List<NitroxId> CollectItemIds(BaseBioReactor reactor)
	{
		List<NitroxId> list = new List<NitroxId>();
		if (reactor.container != null)
		{
			foreach (InventoryItem item in (IEnumerable<InventoryItem>)reactor.container)
			{
				if (item != null && (bool)item.item && item.item.TryGetNitroxId(out NitroxId nitroxId))
				{
					list.Add(nitroxId);
				}
			}
		}
		return list;
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, new PrefixDelegate(Prefix).Method, new Action<BaseBioReactor, List<NitroxId>>(Postfix).Method);
	}
}

