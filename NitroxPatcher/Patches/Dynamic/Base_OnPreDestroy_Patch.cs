using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Before a base gets destroyed, we eventually detach/exit any remote player's object that would be inside so that their GameObjects don't get destroyed
/// </summary>
public sealed partial class Base_OnPreDestroy_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Base t) => t.OnPreDestroy());

    public static void Prefix(Base __instance)
    {
        foreach (RemotePlayerIdentifier remotePlayerIdentifier in __instance.GetComponentsInChildren<RemotePlayerIdentifier>(true))
        {
            remotePlayerIdentifier.RemotePlayer.ResetStates();
        }
    }
}
