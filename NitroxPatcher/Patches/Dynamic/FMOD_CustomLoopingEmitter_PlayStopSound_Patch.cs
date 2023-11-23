using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_CustomLoopingEmitter_PlayStopSound_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomLoopingEmitter t) => t.PlayStopSound());

    public static bool Prefix()
    {
        return !FMODSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMOD_CustomLoopingEmitter __instance)
    {
        if (__instance.assetStop && Resolve<FMODSystem>().IsWhitelisted(__instance.assetStop.path))
        {
            if (__instance.TryGetComponentInParent(out NitroxEntity nitroxEntity))
            {
                Resolve<FMODSystem>().PlayCustomLoopingEmitter(nitroxEntity.Id, __instance.assetStop.path);
            }
        }
    }
}
