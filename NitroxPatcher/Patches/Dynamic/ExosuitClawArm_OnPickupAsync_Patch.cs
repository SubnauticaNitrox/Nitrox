using System.Collections;
using System.Reflection;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitClawArm_OnPickupAsync_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitClawArm t) => t.OnPickupAsync(default, default, default));

    public static IEnumerator Postfix(IEnumerator __result, ExosuitClawArm __instance)
    {
        if (__instance.exosuit.TryGetIdOrWarn(out NitroxId id))
        {
            Pickupable pickupable = __instance.exosuit.GetActiveTarget().GetComponent<Pickupable>();
            PickPrefab pickPrefab = __instance.exosuit.GetActiveTarget().GetComponent<PickPrefab>();

            if (pickupable != null && pickupable.isPickupable && __instance.exosuit.storageContainer.container.HasRoomFor(pickupable))
            {
                NitroxServiceLocator.LocateService<Items>().PickedUp(pickupable.gameObject, pickupable.GetTechType(), id);
            }
            else if (pickPrefab != null)
            {
                NitroxServiceLocator.LocateService<Items>().PickedUp(pickPrefab.gameObject, pickPrefab.pickTech, id);
            }
        }

        Resolve<Items>().PickingUpCount++;

        try
        {
            while (__result.MoveNext())
            {
                yield return __result.Current;
            }
        }
        finally
        {
            Resolve<Items>().PickingUpCount--;
        }
    }
}
