using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Allow creatures to chose remote players as targets only if they're not in creative mode.
/// NB: This doesn't check for other player's use of 'invisible' command
/// </summary>
public sealed partial class AggressiveWhenSeeTarget_IsTargetValid_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((AggressiveWhenSeeTarget t) => t.IsTargetValid(default(GameObject)));

    public static bool Prefix(GameObject target, ref bool __result)
    {
        if (!target)
        {
            return false;
        }
        // We only want to cancel if the remote player is a target but 
        if (target.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier) &&
            remotePlayerIdentifier.RemotePlayer.PlayerContext.GameMode == NitroxModel.Server.NitroxGameMode.CREATIVE)
        {
            __result = false;
            return false;
        }
        return true;
    }
}
