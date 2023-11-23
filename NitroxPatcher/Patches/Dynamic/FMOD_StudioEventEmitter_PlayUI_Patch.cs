using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_StudioEventEmitter_PlayUI_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_StudioEventEmitter t) => t.PlayUI());

    public static bool Prefix()
    {
        return !FMODSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMOD_StudioEventEmitter __instance, float ____lastTimePlayed)
    {
        if (Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path))
        {
            if (____lastTimePlayed == 0.0 || Time.time > ____lastTimePlayed + __instance.minInterval)
            {
                if (__instance.TryGetComponentInParent(out NitroxEntity nitroxEntity))
                {
                    Resolve<FMODSystem>().PlayStudioEmitter(nitroxEntity.Id, __instance.asset.path, true, false);
                }
            }
        }
    }
}
