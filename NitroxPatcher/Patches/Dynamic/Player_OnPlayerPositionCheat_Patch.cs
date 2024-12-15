using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

// This method sets the player's subroot to null, so send a packet accordingly
public sealed partial class Player_OnPlayerPositionCheat_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.OnPlayerPositionCheat());

    public static void Prefix(Player __instance)
    {
        if (__instance.GetCurrentSub())
        {
            Resolve<LocalPlayer>().BroadcastSubrootChange(Optional.Empty);
        }
    }
}
