using System.Collections;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class ExosuitClawArm_OnPickupAsync_Patch : NitroxPatch, IDynamicPatch
{
    private static Items items;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitClawArm t) => t.OnPickupAsync(default, default, default));

    public ExosuitClawArm_OnPickupAsync_Patch(Items i)
    {
        items = i;
    }

    public static IEnumerator Postfix(IEnumerator __result, ExosuitClawArm __instance)
    {
        if (__instance.exosuit.TryGetIdOrWarn(out NitroxId id))
        {
            Pickupable pickupable = __instance.exosuit.GetActiveTarget().GetComponent<Pickupable>();
            PickPrefab pickPrefab = __instance.exosuit.GetActiveTarget().GetComponent<PickPrefab>();

            if (pickupable != null && pickupable.isPickupable && __instance.exosuit.storageContainer.container.HasRoomFor(pickupable))
            {
                items.PickedUp(pickupable.gameObject, pickupable.GetTechType(), id);
            }
            else if (pickPrefab != null)
            {
                items.PickedUp(pickPrefab.gameObject, pickPrefab.pickTech, id);
            }
        }

        items.PickingUpCount++;

        try
        {
            while (__result.MoveNext())
            {
                yield return __result.Current;
            }
        }
        finally
        {
            items.PickingUpCount--;
        }
    }
}
