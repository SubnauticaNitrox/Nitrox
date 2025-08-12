using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitClawArm_OnPickup_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitClawArm t) => t.OnPickup());

    public static bool Prefix(ExosuitClawArm __instance)
    {
        Exosuit componentInParent = __instance.GetComponentInParent<Exosuit>();
        if (componentInParent.GetActiveTarget())
        {
            Pickupable pickupable = componentInParent.GetActiveTarget().GetComponent<Pickupable>();
            PickPrefab component = componentInParent.GetActiveTarget().GetComponent<PickPrefab>();
            if (pickupable != null && pickupable.isPickupable && componentInParent.storageContainer.container.HasRoomFor(pickupable))
            {
                NitroxServiceLocator.LocateService<Items>().PickedUp(pickupable.gameObject, pickupable.GetTechType());
            }
            else if (component != null)
            {
                Log.Debug("Delete Pickprefab for exosuit claw arm");
                NitroxServiceLocator.LocateService<Items>().PickedUp(component.gameObject, component.pickTech);
            }
        }
        return true;
    }
}
