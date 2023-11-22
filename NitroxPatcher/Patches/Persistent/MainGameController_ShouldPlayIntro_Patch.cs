using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public partial class MainGameController_ShouldPlayIntro_Patch : NitroxPatch, IPersistentPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method(() => MainGameController.ShouldPlayIntro());

    public static void Postfix(ref bool __result)
    {
        __result = false;
    }
}
