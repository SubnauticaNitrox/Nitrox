using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Pickupable_Drop_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Pickupable t) => t.Drop(default, default, default));

    public static void Postfix(Pickupable __instance)
    {
        if (PlaceTool_Place_Patch.RunningThisFrame)
        {
            return;
        }
        Resolve<Items>().Dropped(__instance.gameObject, __instance.GetTechType());
    }
}
