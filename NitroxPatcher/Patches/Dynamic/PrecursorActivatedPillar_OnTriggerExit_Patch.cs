using System.Collections.Generic;
using System.Reflection;
using FMOD.Studio;
using NitroxClient.GameLogic.PlayerLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents ion cube pedestals from retracting when a player leaves if other players are still nearby.
/// </summary>
public sealed partial class PrecursorActivatedPillar_OnTriggerExit_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorActivatedPillar t) => t.OnTriggerExit(default));

    /// <summary>
    /// Tracks the number of players currently in each pillar's trigger zone.
    /// </summary>
    private static readonly Dictionary<int, int> playerCountByPillar = [];

    public static void IncrementPlayerCount(PrecursorActivatedPillar pillar)
    {
        int id = pillar.GetInstanceID();
        playerCountByPillar.TryGetValue(id, out int count);
        playerCountByPillar[id] = count + 1;
    }

    public static bool Prefix(PrecursorActivatedPillar __instance, Collider col, ref bool ___extended, ref bool ___isFullyExtended, FMODAsset ___closeSound, FMOD_CustomLoopingEmitter ___openedLoopingSound)
    {
        GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
        if (!entityRoot)
        {
            entityRoot = col.gameObject;
        }

        bool isLocalPlayer = entityRoot.GetComponentInChildren<Player>() != null;
        bool isRemotePlayer = entityRoot.GetComponentInChildren<RemotePlayerIdentifier>() != null;

        if (!isLocalPlayer && !isRemotePlayer)
        {
            return false;
        }

        // Decrement player count (only if we have a count for this pillar)
        int id = __instance.GetInstanceID();
        int newCount = 0;
        if (playerCountByPillar.TryGetValue(id, out int count) && count > 0)
        {
            newCount = count - 1;
            playerCountByPillar[id] = newCount;
        }

        // Only retract if no players remain
        if (newCount <= 0)
        {
            if (___closeSound)
            {
                Utils.PlayFMODAsset(___closeSound, __instance.transform, 20f);
            }
            if (___openedLoopingSound)
            {
                ___openedLoopingSound.Stop(STOP_MODE.ALLOWFADEOUT);
            }
            ___extended = false;
            ___isFullyExtended = true;
        }

        return false;
    }
}
