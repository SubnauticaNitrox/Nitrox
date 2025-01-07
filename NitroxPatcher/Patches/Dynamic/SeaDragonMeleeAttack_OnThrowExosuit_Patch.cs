using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents non simulating players from running locally <see cref="SeaDragonMeleeAttack.OnThrowExosuit"/>.
/// </summary>
public sealed partial class SeaDragonMeleeAttack_OnThrowExosuit_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaDragonMeleeAttack t) => t.OnThrowExosuit(default));

    public static bool Prefix(SeaDragonMeleeAttack __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId) ||
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }

        return false;
    }
}
