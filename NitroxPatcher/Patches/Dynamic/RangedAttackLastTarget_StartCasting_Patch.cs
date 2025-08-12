using System.Reflection;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents non simulating players from running locally <see cref="RangedAttackLastTarget.StartCasting"/>.
/// Broadcasts this event on the simulating player.
/// </summary>
public sealed partial class RangedAttackLastTarget_StartCasting_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((RangedAttackLastTarget t) => t.StartCasting(default));

    public static void Prefix(RangedAttackLastTarget __instance)
    {
        RangedAttackLastTarget_StartCharging_Patch.BroadcastRangedAttack(__instance, RangedAttackLastTargetUpdate.ActionState.CASTING);
    }
}

