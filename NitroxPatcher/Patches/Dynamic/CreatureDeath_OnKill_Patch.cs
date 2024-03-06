using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="CreatureDeath.OnKill"/> from happening on non-simulated entities
/// </summary>
public sealed partial class CreatureDeath_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureDeath t) => t.OnKill());

    public static bool Prefix(CreatureDeath __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId creatureId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }
        return false;
    }
}
