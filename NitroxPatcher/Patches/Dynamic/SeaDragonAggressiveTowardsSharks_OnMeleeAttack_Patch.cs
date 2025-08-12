#if SUBNAUTICA
using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Registers sea dragons' attack on remote players as attacks on a player
/// </summary>
public sealed partial class SeaDragonAggressiveTowardsSharks_OnMeleeAttack_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaDragonAggressiveTowardsSharks t) => t.OnMeleeAttack(default));

    public static bool Prefix(SeaDragonAggressiveTowardsSharks __instance, GameObject target)
    {
        if (target.GetComponent<RemotePlayerIdentifier>())
        {
            __instance.timeLastPlayerAttack = Time.time;
            return false;
        }
        return true;
    }
}
#endif
