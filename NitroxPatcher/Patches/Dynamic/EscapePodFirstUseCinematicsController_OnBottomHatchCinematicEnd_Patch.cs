using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Extensions;
using NitroxClient.GameLogic.Spawning.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class EscapePodFirstUseCinematicsController_OnBottomHatchCinematicEnd_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePodFirstUseCinematicsController t) => t.OnBottomHatchCinematicEnd((CinematicModeEventData)null));

	public static void Postfix(EscapePodFirstUseCinematicsController __instance)
	{
		if (__instance.escapePod.TryGetIdOrWarn(out NitroxId nitroxId, "Postfix", "NitroxPatcher\\Patches\\Dynamic\\EscapePodFirstUseCinematicsController_OnBottomHatchCinematicEnd_Patch.cs", 23))
		{
			EntityMetadata value = NitroxPatch.Resolve<EntityMetadataManager>().Extract(__instance.escapePod.gameObject).Value;
			NitroxPatch.Resolve<IPacketSender>().Send(new EntityMetadataUpdate(nitroxId, value));
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, null, new Action<EscapePodFirstUseCinematicsController>(Postfix).Method);
	}
}


