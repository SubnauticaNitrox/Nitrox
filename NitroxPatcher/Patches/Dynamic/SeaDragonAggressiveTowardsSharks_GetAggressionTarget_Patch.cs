#if SUBNAUTICA
using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Allow sea dragons to chose remote players as targets
/// </summary>
public sealed partial class SeaDragonAggressiveTowardsSharks_GetAggressionTarget_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaDragonAggressiveTowardsSharks t) => t.GetAggressionTarget());

    public static bool Prefix(SeaDragonAggressiveTowardsSharks __instance, ref GameObject __result)
    {
        if (Time.time <= __instance.timeLastPlayerAttack + __instance.playerAttackInterval)
        {
            // Will call base.GetAggressionTarget()
            return true;
        }
        
        // In this method, players are priority targets. This will also account for the local player case
        IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(RemotePlayer.PLAYER_ECO_TARGET_TYPE, __instance.transform.position, __instance.isTargetValidFilter, __instance.maxSearchRings);
        if (ecoTarget != null && __instance.IsTargetValid(ecoTarget.GetGameObject()))
        {
            __result = ecoTarget.GetGameObject();
            return false;
        }

        // To redirect the call to base.GetAggressionTarget(), we ensure the if is skipped in the original method
        float timeLastPlayerAttack = __instance.timeLastPlayerAttack;
        __instance.timeLastPlayerAttack = Time.time;
        
        __result = __instance.GetAggressionTarget();

        __instance.timeLastPlayerAttack = timeLastPlayerAttack;
        return false;
    }
}
#endif
