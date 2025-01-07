using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Inventory_LoseItems_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Inventory t) => t.LoseItems());

    public static bool Prefix(Inventory __instance, ref bool __result)
    {
        if (Resolve<LocalPlayer>().KeepInventoryOnDeath)
        {
            __result = false;
            return false;
        }
        return true;
    }
}
