using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Vehicle_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        private static bool skipPrefix = false;

        public static bool Prefix(Vehicle __instance, GUIHand hand)
        {
            if (skipPrefix)
            {
                return true;
            }
            
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
            
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on the vehicle: {id}");
                return true;
            }

            HandInteraction<Vehicle> context = new HandInteraction<Vehicle>(__instance, hand);
            LockRequest<HandInteraction<Vehicle>> lockRequest = new LockRequest<HandInteraction<Vehicle>>(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

            simulationOwnership.RequestSimulationLock(lockRequest);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<Vehicle> context)
        {
            Vehicle vehicle = context.Target;

            if (lockAquired)
            {
                skipPrefix = true;
                TARGET_METHOD.Invoke(vehicle, new[] { context.GuiHand });
                skipPrefix = false;
            }
            else
            {
                vehicle.gameObject.AddComponent<DenyOwnershipHand>();
                vehicle.isValidHandTarget = false;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
