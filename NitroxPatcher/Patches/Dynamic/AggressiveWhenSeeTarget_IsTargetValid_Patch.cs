using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Allow creatures to choose remote players as targets only if they can be attacked (<see cref="RemotePlayer.CanBeAttacked"/>)
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
        // We only want to cancel if the target is a remote player which can't be attacked
        if (target.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier) &&
            !remotePlayerIdentifier.RemotePlayer.CanBeAttacked())
        {
            __result = false;
            return false;
        }
        return true;
    }
}
