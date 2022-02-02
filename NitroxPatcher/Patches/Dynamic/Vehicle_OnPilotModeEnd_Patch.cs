using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Vehicle_OnPilotModeEnd_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Vehicle t) => t.OnPilotModeEnd());

        public static void Prefix(Vehicle __instance)
        {
            Resolve<Vehicles>().BroadcastOnPilotModeChanged(__instance, false);
            // Fixes instances of vehicles stuck on nothing by forcing the workaround (let another player enter and leave the vehicle)
            if (__instance.TryGetComponent(out MultiplayerVehicleControl mvc))
            {
                mvc.Exit();
            }

            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
