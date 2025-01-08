#if DEBUG
using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MainGameController_ShouldPlayIntro_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => MainGameController.ShouldPlayIntro());

    public static void Postfix(ref bool __result)
    {
        __result = false;
        uGUI_SceneIntro_IntroSequence_Patch.SkipLocalCinematic(uGUI.main.intro);
    }
}
#endif
