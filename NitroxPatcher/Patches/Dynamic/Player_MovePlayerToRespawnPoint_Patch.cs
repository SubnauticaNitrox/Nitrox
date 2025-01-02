using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcast the escape pod and sub root changes of a player respawning.
/// </summary>
public sealed partial class Player_MovePlayerToRespawnPoint_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.MovePlayerToRespawnPoint());

    public static void Postfix(Player __instance)
    {
        Optional<NitroxId> currentSubId = Optional.Empty;
        if (__instance.currentSub)
        {
            currentSubId = __instance.currentSub.GetId();
        }

        Optional<NitroxId> currentEscapePodId = Optional.Empty;
        if (__instance.currentEscapePod)
        {
            currentEscapePodId = __instance.currentEscapePod.GetId();
        }

        Resolve<LocalPlayer>().BroadcastSubrootChange(currentSubId);
        Resolve<LocalPlayer>().BroadcastEscapePodChange(currentEscapePodId);
    }
}
