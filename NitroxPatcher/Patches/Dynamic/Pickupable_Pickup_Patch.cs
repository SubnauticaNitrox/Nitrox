using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Pickupable_Pickup_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Pickupable p) => p.Pickup(default));

    public static void Prefix(Pickupable __instance)
    {
        Resolve<Items>().PickedUp(__instance.gameObject, __instance.GetTechType());
    }
}

