using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Floater_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Floater t) => t.FixedUpdate());

    public static bool Prefix(Floater __instance)
    {
        if (!__instance.fixedJoint || !__instance.fixedJoint.connectedBody ||
            !__instance.fixedJoint.connectedBody.TryGetNitroxId(out NitroxId jointId))
        {
            return true;
        }

        Rigidbody connectedBody = __instance.fixedJoint.connectedBody;

        // For now we only check for vehicles which are our main concern but in the future when we properly sync floaters,
        // we'll extend the check to all entities
        if (!connectedBody.GetComponent<Vehicle>() || !connectedBody.GetComponent<NitroxCyclops>())
        {
            return true;
        }

        // We only apply the FixedUpdate when we have a lock type on it (true)
        return Resolve<SimulationOwnership>().HasAnyLockType(jointId);
    }
}
