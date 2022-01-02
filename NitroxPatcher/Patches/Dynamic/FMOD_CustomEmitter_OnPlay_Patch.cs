using System.Reflection;
using FMOD.Studio;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public class FMOD_CustomEmitter_OnPlay_Patch : NitroxPatch, IDynamicPatch
{
    private static FMODSystem fmodSystem;

    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomEmitter t) => t.OnPlay());

    public static bool Prefix()
    {
        return !FMODSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMOD_CustomEmitter __instance)
    {
        if (fmodSystem.IsWhitelisted(__instance.asset.path, out bool isGlobal, out float radius))
        {
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
                    fmodSystem.PlayCustomEmitter(nitroxEntity.Id, __instance.asset.path, true);
                }
                else
                {
                    fmodSystem.PlayEventInstance(nitroxEntity.Id, __instance.asset.path, true, __instance.transform.position.ToDto(), volume, radius, isGlobal);
                }
            }
            else
            {
                fmodSystem.PlayAsset(__instance.asset.path, __instance.transform.position.ToDto(), volume, radius, isGlobal);
            }
        }
    }

    public override void Patch(Harmony harmony)
    {
        fmodSystem = Resolve<FMODSystem>();
        PatchMultiple(harmony, TARGET_METHOD, prefix: true, postfix: true);
    }
}
