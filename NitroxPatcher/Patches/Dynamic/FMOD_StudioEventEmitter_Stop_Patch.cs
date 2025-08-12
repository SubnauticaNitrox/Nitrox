using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_StudioEventEmitter_Stop_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_StudioEventEmitter t) => t.Stop(default(bool)));

    public static bool Prefix()
    {
        return !FMODSoundSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMOD_StudioEventEmitter __instance, bool allowFadeout)
    {
        if (!__instance.evt.hasHandle())
        {
            return;
        }

        if (!Resolve<FMODWhitelist>().IsWhitelisted(__instance.asset.path))
        {
            return;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity nitroxEntity, true))
        {
            Log.Warn($"[{nameof(FMOD_StudioEventEmitter_Stop_Patch)}] - No NitroxEntity found for {__instance.asset.path} at {__instance.GetFullHierarchyPath()}");
            return;
        }

        Resolve<FMODSystem>().SendStudioEmitterStop(nitroxEntity.Id, __instance.asset.path, allowFadeout);
    }
}
