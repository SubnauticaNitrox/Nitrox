using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SubNameInput_OnNameChange_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubNameInput t) => t.OnNameChange(default(string)));

    public static void Postfix(SubNameInput __instance)
    {
        if (TryGetTargetId(__instance, out object target, out NitroxId targetId))
        {
            Resolve<Entities>().EntityMetadataChanged(target, targetId);
        }
    }

    public static bool TryGetTargetId(SubNameInput subNameInput, out object target, out NitroxId targetId)
    {
#if SUBNAUTICA
        SubName subName = subNameInput.target;
        if (!subName)
#elif BELOWZERO
        ICustomizeable subName = subNameInput.target;
        if (subName == null)
#endif
        {
            target = null;
            targetId = null;
            return false;
        }
        Log.Debug($"SubName is {subName.GetName()} {subName.GetType()}");
        //TODO: work out how to get the vehicle object from the ICustomizeable
#if SUBNAUTICA
        if (subName.TryGetComponent(out Vehicle vehicle))
        {
            target = vehicle;
            return vehicle.TryGetNitroxId(out targetId);
        }
        else if (subName.TryGetComponentInParent(out Rocket rocket, true))
        {
            // For some reason only the rocket has a full functioning ghost with a different NitroxId when spawning/constructing, so we are ignoring it.
            if (rocket.TryGetComponentInChildren(out VFXConstructing constructing, true) && !constructing.isDone)
            {
                target = null;
                targetId = null;
                return false;
            }
        }
#endif
        // Cyclops and Rocket has their SubNameInput and SubName in the same GameObject, marked with a NitroxEntity
        target = subNameInput;
        return subNameInput.TryGetNitroxId(out targetId);
    }
}
