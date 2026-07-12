using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class Bed_ExitInUseMode_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	public static readonly MethodInfo TARGET_METHOD = typeof(Bed).GetMethod("ExitInUseMode", BindingFlags.Instance | BindingFlags.NonPublic);

	public static void Prefix(Bed __instance)
	{
		if (__instance.inUseMode == Bed.InUseMode.Sleeping)
		{
			string animationKey = ((__instance.currentStandUpCinematicController == __instance.leftStandUpCinematicController) ? "bed_up_left" : "bed_up_right");
			if (__instance.TryGetNitroxId(out NitroxId nitroxId))
			{
				NitroxPatch.Resolve<IPacketSender>().Send(new BedExitAnimation(NitroxPatch.Resolve<LocalPlayer>().SessionId.Value, nitroxId, animationKey));
				NitroxPatch.Resolve<SimulationOwnership>().RequestSimulationLock(nitroxId, SimulationLockType.TRANSIENT);
			}
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, new Action<Bed>(Prefix).Method);
	}
}


