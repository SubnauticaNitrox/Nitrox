#if DEBUG
using System.Reflection;
using NitroxModel.Helper;
using NitroxPatcher.Patches.Dynamic;

namespace NitroxPatcher.Patches.Persistent;

public sealed partial class MainGameController_ShouldPlayIntro_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => MainGameController.ShouldPlayIntro());

    public static void Postfix(ref bool __result)
    {
        __result = false;
        uGUI_SceneIntro_IntroSequence_Patch.SkipLocalCinematic(uGUI.main.intro);
    }
}
#endif
