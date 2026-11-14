using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.Unity.Helper;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Releases the exclusive lock on bed exit.
/// This handles both manual exit (pressing E) and automatic exit (sleep cycle completion).
/// </summary>
public sealed partial class Bed_ExitInUseMode_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = typeof(Bed).GetMethod("ExitInUseMode", BindingFlags.NonPublic | BindingFlags.Instance);

    public static void Prefix(Bed __instance)
    {
        if (__instance.inUseMode != Bed.InUseMode.Sleeping)
        {
            return;
        }

        if (__instance.currentStandUpCinematicController.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            Resolve<SimulationOwnership>().RequestSimulationLock(BedLockId.Resolve(entity, __instance.currentStandUpCinematicController), SimulationLockType.TRANSIENT);
        }
    }
}
