using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ArmsController_Update_SeamothPassenger_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ArmsController t) => t.Update());

    public static void Postfix(ArmsController __instance)
    {
        Resolve<SeamothPassengers>().MaintainLocalSeatedAnimation(__instance);
    }
}
