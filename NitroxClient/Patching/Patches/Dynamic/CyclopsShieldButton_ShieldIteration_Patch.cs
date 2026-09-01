using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class CyclopsShieldButton_ShieldIteration_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsShieldButton t) => t.ShieldIteration());

    public static bool Prefix(CyclopsShieldButton __instance)
    {
        // Only the owner should run this method so the power drains at the correct speed
        return __instance.subRoot.TryGetNitroxId(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id);
    }
}
