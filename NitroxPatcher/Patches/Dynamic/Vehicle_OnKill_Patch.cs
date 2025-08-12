using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Vehicle_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Vehicle t) => t.OnKill());

    public static void Prefix(Vehicle __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasExclusiveLock(id))
        {
            Resolve<SimulationOwnership>().StopSimulatingEntity(id);
            Resolve<Vehicles>().BroadcastDestroyedVehicle(id);
        }
        foreach (RemotePlayerIdentifier identifier in __instance.GetComponentsInChildren<RemotePlayerIdentifier>(true))
        {
            identifier.RemotePlayer.ResetStates();
        }
    }
}
