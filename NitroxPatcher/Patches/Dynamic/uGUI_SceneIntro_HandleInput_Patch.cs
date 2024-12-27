using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_SceneIntro_HandleInput_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SceneIntro t) => ((IInputHandler) t).HandleInput());

    public static void Postfix(uGUI_SceneIntro __instance, ref bool __result)
    {
        if (!__result)
        {
            // Altering return result when skipping waiting for player
            if (uGUI_SceneIntro_IntroSequence_Patch.IsWaitingForPartner || Resolve<LocalPlayer>().IntroCinematicMode == IntroCinematicMode.SINGLEPLAYER)
            {
                __result = true;
            }
        }
    }
}
