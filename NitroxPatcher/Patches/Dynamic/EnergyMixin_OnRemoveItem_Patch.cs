using System.Reflection;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Local-only bookkeeping: keeps <see cref="BatteryChildEntityHelper"/>'s pending-installed-battery tracking in sync
/// with actual battery removals, so a later pickup doesn't resurrect a battery that isn't there anymore. This does not
/// broadcast anything over the network; item battery state is still fully (re-)derived on pickup/drop, as before.
/// </summary>
public sealed partial class EnergyMixin_OnRemoveItem_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EnergyMixin t) => t.OnRemoveItem(default(InventoryItem)));

    public static void Postfix(EnergyMixin __instance, InventoryItem item)
    {
        NitroxEntity parent = __instance.gameObject.FindAncestor<NitroxEntity>();
        if (item == null || !parent)
        {
            return;
        }

        BatteryChildEntityHelper.ForgetPendingInstalledBattery(__instance, parent.Id);
    }
}
