using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public partial class MainGameController_ShouldPlayIntro_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => MainGameController.ShouldPlayIntro());

    public static void Postfix(ref bool __result)
    {
        __result = false;
    }
}
