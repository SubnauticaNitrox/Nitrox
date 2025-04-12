using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;


/// <summary>
/// Prevents non simulating players from running locally <see cref="MeleeAttack.OnTouch(Collider)"/>.
/// 
/// Adds RemotePlayer support for the simulating player
/// </summary>
public sealed partial class MeleeAttack_OnTouch_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((MeleeAttack t) => t.OnTouch(default));

    public static bool Prefix(MeleeAttack __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId) ||
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }

        return false;
    }

    // TODO: Add transpiler to add support for held item eat from remote players, we might need to add an equivalent of Inventory.GetHeldItem()
    // For the MeleeAttack part, it'll natively work since RemotePlayer should have a LiveMixin
}
