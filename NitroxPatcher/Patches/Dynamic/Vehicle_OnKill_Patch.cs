using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Vehicle_OnKill_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Vehicle t) => t.OnKill());

        public static void Prefix(Vehicle __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            if (Resolve<SimulationOwnership>().HasExclusiveLock(id))
            {
                Resolve<SimulationOwnership>().StopSimulatingEntity(id);
                Resolve<Vehicles>().BroadcastDestroyedVehicle(__instance);
            }
            foreach (RemotePlayerIdentifier identifier in __instance.GetComponentsInChildren<RemotePlayerIdentifier>(true))
            {
                identifier.RemotePlayer.ResetStates();
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
