using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public class MainGameController_ShouldPlayIntro_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => MainGameController.ShouldPlayIntro());

    public static void Postfix(ref bool __result)
    {
        __result = false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
