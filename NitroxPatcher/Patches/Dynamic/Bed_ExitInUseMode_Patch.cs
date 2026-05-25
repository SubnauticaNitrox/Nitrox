using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Sends stand-up animation packet when exiting bed and releases the exclusive lock.
/// This handles both manual exit (pressing E) and automatic exit (sleep cycle completion).
/// </summary>
public sealed partial class Bed_ExitInUseMode_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = typeof(Bed).GetMethod("ExitInUseMode", BindingFlags.NonPublic | BindingFlags.Instance);

    public static void Prefix(Bed __instance)
    {
        // Only send packet if we're actually sleeping
        if (__instance.inUseMode != Bed.InUseMode.Sleeping)
        {
            return;
        }

        // Determine which stand-up animation based on current cinematic controller
        string animationKey = __instance.currentStandUpCinematicController == __instance.leftStandUpCinematicController
            ? "bed_up_left"
            : "bed_up_right";

        if (__instance.TryGetNitroxId(out NitroxId bedId))
        {
            Resolve<IPacketSender>().Send(new BedExitAnimation(Resolve<LocalPlayer>().SessionId.Value, bedId, animationKey));
            // Release exclusive lock when exiting bed
            Resolve<SimulationOwnership>().RequestSimulationLock(bedId, SimulationLockType.TRANSIENT);
        }
    }
}
