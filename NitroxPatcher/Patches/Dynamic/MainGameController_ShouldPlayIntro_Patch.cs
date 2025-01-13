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
        EscapePod.main.DamageRadio(); // For explanation see similar code in uGUI_SceneIntro_HandleInput_Patch.
        uGUI_SceneIntro_IntroSequence_Patch.SkipLocalCinematic(uGUI.main.intro);
    }
}
#endif
