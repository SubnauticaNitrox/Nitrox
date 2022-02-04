using System.Reflection;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public class FMOD_CustomEmitter_OnStop_Patch : NitroxPatch, IDynamicPatch
{
    private static FMODSystem fmodSystem;

    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomEmitter t) => t.OnStop());

    public static void Postfix(FMOD_CustomEmitter __instance)
    {
        if (!fmodSystem.IsWhitelisted(__instance.asset.path, out bool isGlobal, out float radius))
        {
            return;
        }
        EventInstance evt = __instance.GetEventInstance();
        evt.getDescription(out EventDescription description);
        evt.getVolume(out float volume, out float _);
        description.is3D(out bool is3D);

        if (!__instance.TryGetComponent(out NitroxEntity nitroxEntity))
        {
            nitroxEntity = __instance.GetComponentInParent<NitroxEntity>();
        }

        if (nitroxEntity)
        {
            if (is3D)
            {
                fmodSystem.PlayCustomEmitter(nitroxEntity.Id, __instance.asset.path, false);
            }
            else
            {
                fmodSystem.PlayEventInstance(nitroxEntity.Id, __instance.asset.path, false, __instance.transform.position.ToDto(), volume, radius, isGlobal);
            }
        }
    }

    public override void Patch(Harmony harmony)
    {
        fmodSystem = Resolve<FMODSystem>();
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
