using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Makes precursor key terminal pedestals (tablet pedestals) react to remote players.
/// </summary>
public sealed partial class PrecursorKeyTerminalTrigger_OnTriggerEnter_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorKeyTerminalTrigger t) => t.OnTriggerEnter(default));

    public static bool Prefix(PrecursorKeyTerminalTrigger __instance, Collider col)
    {
        bool isLocalPlayer = col.gameObject.Equals(Player.main.gameObject);
        bool isRemotePlayer = col.GetComponentInParent<RemotePlayerIdentifier>() != null;

        if (!isLocalPlayer && !isRemotePlayer)
        {
            return false;
        }

        // Track player count for proper exit handling
        PrecursorKeyTerminalTrigger_OnTriggerExit_Patch.IncrementPlayerCount(__instance);

        // Only send OpenDeck if this is the first player entering
        if (PrecursorKeyTerminalTrigger_OnTriggerExit_Patch.GetPlayerCount(__instance) == 1)
        {
            __instance.SendMessageUpwards("OpenDeck");
        }

        return false;
    }
}
