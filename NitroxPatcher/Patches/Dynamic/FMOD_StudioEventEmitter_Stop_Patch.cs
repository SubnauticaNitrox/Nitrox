using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_StudioEventEmitter_Stop_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_StudioEventEmitter t) => t.Stop(default(bool)));

    public static bool Prefix()
    {
        return !FMODSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMOD_StudioEventEmitter __instance, bool allowFadeout)
    {
        if (Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path))
        {
            if (__instance.TryGetComponentInParent(out NitroxEntity nitroxEntity))
            {
                Resolve<FMODSystem>().PlayStudioEmitter(nitroxEntity.Id, __instance.asset.path, false, allowFadeout);
            }
        }
    }
}
