using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FMOD_CustomLoopingEmitter_OnPlay_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FMOD_CustomLoopingEmitter t) => t.OnPlay());

    public static bool Prefix()
    {
        return !FMODSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMOD_CustomLoopingEmitter __instance)
    {
        if (__instance.assetStart && Resolve<FMODSystem>().IsWhitelisted(__instance.assetStart.path))
        {
            if (__instance.TryGetComponentInParent(out NitroxEntity nitroxEntity))
            {
                Resolve<FMODSystem>().PlayCustomLoopingEmitter(nitroxEntity.Id, __instance.assetStart.path);
            }
        }
    }
}
