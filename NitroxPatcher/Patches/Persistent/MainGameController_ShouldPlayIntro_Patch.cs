using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public class MainGameController_ShouldPlayIntro_Patch : NitroxPatch, IPersistentPatch
{
#if SUBNAUTICA
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => MainGameController.ShouldPlayIntro());
#elif BELOWZERO
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IntroVignette t) => t.ShouldPlayIntro());
#endif

    public static void Postfix(ref bool __result)
    {
        __result = false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
