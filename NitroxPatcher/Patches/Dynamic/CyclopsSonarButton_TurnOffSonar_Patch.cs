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

public sealed class CyclopsSonarButton_TurnOffSonar_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarButton t) => t.TurnOffSonar());

	public static void Postfix(CyclopsSonarButton __instance)
	{
		if (!PacketSuppressor<EntityMetadataUpdate>.IsSuppressed && __instance.subRoot.TryGetIdOrWarn(out NitroxId nitroxId, "Postfix", "NitroxPatcher\\Patches\\Dynamic\\CyclopsSonarButton_TurnOffSonar_Patch.cs", 27))
		{
			NitroxPatch.Resolve<Cyclops>().BroadcastMetadataChange(nitroxId);
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, null, new Action<CyclopsSonarButton>(Postfix).Method);
	}
}


