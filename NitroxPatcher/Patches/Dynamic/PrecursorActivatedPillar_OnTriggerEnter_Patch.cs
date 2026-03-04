using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Makes ion cube pedestals (the ones with ion cubes on top that rise up) react to remote players.
/// </summary>
public sealed partial class PrecursorActivatedPillar_OnTriggerEnter_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorActivatedPillar t) => t.OnTriggerEnter(default));

    public static bool Prefix(PrecursorActivatedPillar __instance, Collider col, bool ___active, ref bool ___extended, ref bool ___isFullyExtended, FMODAsset ___openSound, FMOD_CustomLoopingEmitter ___openedLoopingSound)
    {
        if (!___active)
        {
            return false;
        }

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

        // Track player count for proper exit handling
        PrecursorActivatedPillar_OnTriggerExit_Patch.IncrementPlayerCount(__instance);

        // Only play sounds and animate if not already extended
        if (!___extended)
        {
            if (___openSound)
            {
                Utils.PlayFMODAsset(___openSound, __instance.transform, 20f);
            }
            if (___openedLoopingSound)
            {
                ___openedLoopingSound.Play();
            }
            ___extended = true;
            ___isFullyExtended = false;
        }

        return false;
    }
}
