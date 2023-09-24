using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_CustomLoopingEmitter_PlayStopSound_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomLoopingEmitter t) => t.PlayStopSound());

    public static bool Prefix()
    {
        return !FMODSoundSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMOD_CustomLoopingEmitter __instance)
    {
        if (!__instance.assetStop || !Resolve<FMODWhitelist>().IsWhitelisted(__instance.assetStop.path))
        {
            return;
        }

        if (__instance.TryGetComponentInParent(out NitroxEntity nitroxEntity, true))
        {
            Resolve<FMODSystem>().SendCustomLoopingEmitterPlay(nitroxEntity.Id, __instance.assetStop.path);
        }
    }
}
