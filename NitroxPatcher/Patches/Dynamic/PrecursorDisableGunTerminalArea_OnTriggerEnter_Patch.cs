using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Makes the gun disable terminal open when remote players approach.
/// </summary>
public sealed partial class PrecursorDisableGunTerminalArea_OnTriggerEnter_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorDisableGunTerminalArea t) => t.OnTriggerEnter(default));

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

        // Track player count for proper exit handling
        PrecursorDisableGunTerminalArea_OnTriggerExit_Patch.IncrementPlayerCount(__instance);

        // Only open if this is the first player entering
        if (PrecursorDisableGunTerminalArea_OnTriggerExit_Patch.GetPlayerCount(__instance) == 1)
        {
            __instance.terminal.OnTerminalAreaEnter();
        }

        return false;
    }
}
