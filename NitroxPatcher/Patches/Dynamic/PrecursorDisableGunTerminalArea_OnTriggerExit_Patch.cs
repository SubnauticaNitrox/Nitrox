using System.Collections.Generic;
using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents the gun disable terminal from closing when a player leaves if other players are still nearby.
/// </summary>
public sealed partial class PrecursorDisableGunTerminalArea_OnTriggerExit_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorDisableGunTerminalArea t) => t.OnTriggerExit(default));

    /// <summary>
    /// Tracks the number of players currently in each trigger zone.
    /// </summary>
    private static readonly Dictionary<int, int> playerCountByTrigger = [];

    public static void IncrementPlayerCount(PrecursorDisableGunTerminalArea trigger)
    {
        int id = trigger.GetInstanceID();
        playerCountByTrigger.TryGetValue(id, out int count);
        playerCountByTrigger[id] = count + 1;
    }

    public static int GetPlayerCount(PrecursorDisableGunTerminalArea trigger)
    {
        int id = trigger.GetInstanceID();
        playerCountByTrigger.TryGetValue(id, out int count);
        return count;
    }

    public static bool Prefix(PrecursorDisableGunTerminalArea __instance, Collider other)
    {
        GameObject entityRoot = UWE.Utils.GetEntityRoot(other.gameObject);
        if (entityRoot == null)
        {
            entityRoot = other.gameObject;
        }

        bool isLocalPlayer = entityRoot.GetComponent<Player>() != null;
        bool isRemotePlayer = entityRoot.GetComponent<RemotePlayerIdentifier>() != null;

        if (!isLocalPlayer && !isRemotePlayer)
        {
            return false;
        }

        // Decrement player count (only if we have a count for this trigger)
        int id = __instance.GetInstanceID();
        int newCount = 0;
        if (playerCountByTrigger.TryGetValue(id, out int count) && count > 0)
        {
            newCount = count - 1;
            playerCountByTrigger[id] = newCount;
        }

        // Only close if no players remain
        if (newCount <= 0)
        {
            __instance.terminal.OnTerminalAreaExit();
        }

        return false;
    }
}
