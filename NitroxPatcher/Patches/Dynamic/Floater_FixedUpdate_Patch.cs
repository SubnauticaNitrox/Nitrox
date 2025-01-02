using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

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

        // If target has an id, we only apply the FixedUpdate when we have a lock type on it (true)
        return Resolve<SimulationOwnership>().HasAnyLockType(jointId);
    }
}
