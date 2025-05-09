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

        Resolve<LocalPlayer>().BroadcastSubrootChange(currentSubId);
        
        // BroadcastEscapePodChange() is handled by EscapePod_RespawnPlayer_Patch for cross-functionality with the warpme command 
    }
}
