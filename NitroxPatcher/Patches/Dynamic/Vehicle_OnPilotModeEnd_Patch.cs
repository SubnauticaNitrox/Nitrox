using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Vehicle_OnPilotModeEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Vehicle t) => t.OnPilotModeEnd());

    public static void Prefix(Vehicle __instance)
    {
        Resolve<Vehicles>().BroadcastOnPilotModeChanged(__instance.gameObject, false);

        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);
        }
    }
}
