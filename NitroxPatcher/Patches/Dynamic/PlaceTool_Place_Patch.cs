using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PlaceTool_Place_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PlaceTool t) => t.Place());
    public static bool RunningThisFrame;

    public static void Prefix()
    {
        RunningThisFrame = true;
    }

    public static void Postfix(PlaceTool __instance, bool __result)
    {
        if (__result && __instance.TryGetComponent(out Pickupable pickupable))
        {
            Resolve<Items>().Placed(__instance.gameObject, pickupable.GetTechType());
        }
    }

    public static void Finalizer()
    {
        RunningThisFrame = false;
    }
}
