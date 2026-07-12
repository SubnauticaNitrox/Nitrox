using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class MapRoomFunctionality_StartScanning_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomFunctionality t) => t.StartScanning(TechType.None));

	public static void Postfix(MapRoomFunctionality __instance)
	{
		if (!PacketSuppressor<EntityMetadataUpdate>.IsSuppressed && __instance.TryGetNitroxId(out NitroxId nitroxId))
		{
			NitroxPatch.Resolve<Entities>().EntityMetadataChangedThrottled(__instance, nitroxId);
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, null, new Action<MapRoomFunctionality>(Postfix).Method);
	}
}


