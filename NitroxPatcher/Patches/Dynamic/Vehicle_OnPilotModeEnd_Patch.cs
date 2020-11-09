using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Vehicle_OnPilotModeEnd_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPilotModeEnd", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(Vehicle __instance)
        {
            NitroxServiceLocator.LocateService<Vehicles>().BroadcastOnPilotModeChanged(__instance, false);

            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT, null);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
