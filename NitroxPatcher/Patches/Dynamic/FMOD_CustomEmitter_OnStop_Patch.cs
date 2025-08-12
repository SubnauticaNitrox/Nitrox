using System.Reflection;
using FMOD.Studio;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_CustomEmitter_OnStop_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomEmitter t) => t.OnStop());

    public static void Postfix(FMOD_CustomEmitter __instance)
    {
        if (!Resolve<FMODWhitelist>().IsWhitelisted(__instance.asset.path))
        {
            return;
        }

        if (__instance.TryGetComponentInParent(out NitroxEntity nitroxEntity, true))
        {
            EventInstance evt = __instance.GetEventInstance();
            evt.getVolume(out float volume, out float _);
            evt.getDescription(out EventDescription description);
            description.is3D(out bool is3D);

            if (is3D)
            {
                Resolve<FMODSystem>().SendCustomEmitterStop(nitroxEntity.Id, __instance.asset.path);
            }
            else
            {
                Resolve<FMODSystem>().SendEventInstanceStop(nitroxEntity.Id, __instance.asset.path, __instance.transform.position.ToDto(), volume);
            }
        }
    }
}
