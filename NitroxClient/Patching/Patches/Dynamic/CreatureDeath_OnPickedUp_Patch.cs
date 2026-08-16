using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
///     Prevents <see cref="CreatureDeath.OnPickedUp" /> from happening on non-simulated entities
/// </summary>
internal sealed partial class CreatureDeath_OnPickedUp_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureDeath t) => t.OnPickedUp(default));

    public CreatureDeath_OnPickedUp_Patch(SimulationOwnership so)
    {
        simulationOwnership = so ?? throw new ArgumentNullException(nameof(so));
    }

    public static bool Prefix(CreatureDeath __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId creatureId) &&
            simulationOwnership.HasAnyLockType(creatureId))
        {
            return true;
        }
        return false;
    }
}
