using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Inventory_Pickup_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Inventory t) => t.Pickup(default, default));

    public static void Prefix()
    {
        Resolve<Items>().IsInventoryPickingUp = true;
    }

    public static void Finalizer()
    {
        Resolve<Items>().IsInventoryPickingUp = false;
    }
}
