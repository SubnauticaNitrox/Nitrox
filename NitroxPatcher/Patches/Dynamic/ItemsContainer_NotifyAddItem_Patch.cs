using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcast items being added to containers except for:
/// - planters when they aren't subscribed to item adding 
/// </summary>
public sealed partial class ItemsContainer_NotifyAddItem_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ItemsContainer t) => t.NotifyAddItem(default));

    public static void Postfix(ItemsContainer __instance, InventoryItem item)
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted || item == null)
        {
            return;
        }

        // If the container is a planter (we can tell thanks to containerType) we only broadcast if the Planter has subscribed to onAddItem
        // otherwise it means that there is an "unofficial" operation which doesn't need broadcasting
        if (__instance.tr.parent && __instance.tr.parent.TryGetComponent(out Planter planter) && !planter.subscribed)
        {
            return;
        }

        Resolve<ItemContainers>().BroadcastItemAdd(item.item, __instance.tr, __instance);
    }
}
