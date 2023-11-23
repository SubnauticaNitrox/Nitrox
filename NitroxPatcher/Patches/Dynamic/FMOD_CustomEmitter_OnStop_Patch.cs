using System.Reflection;
using FMOD.Studio;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_CustomEmitter_OnStop_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomEmitter t) => t.OnStop());

    public static void Postfix(FMOD_CustomEmitter __instance)
    {
        if (Resolve<FMODSystem>().IsWhitelisted(__instance.asset.path))
        {
            __instance.GetEventInstance().getDescription(out EventDescription description);
            description.is3D(out bool is3D);

            if (is3D)
            {
                if (__instance.TryGetComponentInParent(out NitroxEntity nitroxEntity))
                {
                    Resolve<FMODSystem>().PlayCustomEmitter(nitroxEntity.Id, __instance.asset.path, false);
                }
            }
        }
    }
}
