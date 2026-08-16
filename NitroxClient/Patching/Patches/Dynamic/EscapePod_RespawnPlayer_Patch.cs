using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class EscapePod_RespawnPlayer_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.RespawnPlayer());

    public static void Postfix(EscapePod __instance)
    {
        // EscapePod.RespawnPlayer() runs both for player respawn (Player.MovePlayerToRespawnPoint()) and for warpme command
        Optional<NitroxId> id = __instance.GetId();
        Resolve<LocalPlayer>().BroadcastEscapePodChange(id);
    }
}
