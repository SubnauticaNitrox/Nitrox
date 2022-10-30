using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Before a base gets destroyed, we eventually detach/exit any remote player's object that would be inside so that their GameObjects don't get destroyed
/// </summary>
public class Base_OnPreDestroy_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Base t) => t.OnPreDestroy());

    public static void Prefix(Base __instance)
    {
        foreach (RemotePlayerIdentifier remotePlayerIdentifier in __instance.GetComponentsInChildren<RemotePlayerIdentifier>(true))
        {
            remotePlayerIdentifier.RemotePlayer.Detach();
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
