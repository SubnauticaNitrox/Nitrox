using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours.Gui.HUD;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class Bed_OnHandClick_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bed t) => t.OnHandClick(null));

	private static bool skipPrefix;

	public static bool Prefix(Bed __instance, GUIHand hand)
	{
		if (skipPrefix)
		{
			return true;
		}
		if (!__instance.TryGetIdOrWarn(out NitroxId nitroxId, "Prefix", "NitroxPatcher\\Patches\\Dynamic\\Bed_OnHandClick_Patch.cs", 26))
		{
			return true;
		}
		if (NitroxPatch.Resolve<SimulationOwnership>().HasExclusiveLock(nitroxId))
		{
			return true;
		}
		HandInteraction<Bed> context = new HandInteraction<Bed>(__instance, hand);
		LockRequest<HandInteraction<Bed>> lockRequest = new LockRequest<HandInteraction<Bed>>(nitroxId, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);
		NitroxPatch.Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);
		return false;
	}

	private static void ReceivedSimulationLockResponse(NitroxId bedId, bool lockAcquired, HandInteraction<Bed> context)
	{
		Bed target = context.Target;
		if (lockAcquired)
		{
			skipPrefix = true;
			target.OnHandClick(context.GuiHand);
			skipPrefix = false;
			string animationKey = ((target.cinematicController == target.leftLieDownCinematicController) ? "bed_down_left" : "bed_down_right");
			NitroxPatch.Resolve<IPacketSender>().Send(new BedEnterAnimation(NitroxPatch.Resolve<LocalPlayer>().SessionId.Value, bedId, animationKey));
		}
		else
		{
			target.gameObject.AddComponent<DenyOwnershipHand>();
			ErrorMessage.AddMessage("Another player is using this bed");
		}
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, new Func<Bed, GUIHand, bool>(Prefix).Method);
	}
}


