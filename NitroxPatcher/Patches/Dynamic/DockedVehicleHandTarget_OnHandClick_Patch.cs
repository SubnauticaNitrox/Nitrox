using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class DockedVehicleHandTarget_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(DockedVehicleHandTarget);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        private static bool skipPrefix = false;

        public static bool Prefix(DockedVehicleHandTarget __instance, GUIHand hand)
        {
            Vehicle vehicle = __instance.dockingBay.GetDockedVehicle();

            if (skipPrefix || vehicle == null)
            {
                return true;
            }

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            NitroxId id = NitroxEntity.GetId(vehicle.gameObject);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on this vehicle: {id}");
                return true;
            }

            HandInteraction<DockedVehicleHandTarget> context = new HandInteraction<DockedVehicleHandTarget>(__instance, hand);
            LockRequest<HandInteraction<DockedVehicleHandTarget>> lockRequest = new LockRequest<HandInteraction<DockedVehicleHandTarget>>(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

            simulationOwnership.RequestSimulationLock(lockRequest);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<DockedVehicleHandTarget> context)
        {
            if (lockAquired)
            {
                VehicleDockingBay dockingBay = context.Target.dockingBay;
                NitroxServiceLocator.LocateService<Vehicles>().BroadcastVehicleUndocking(dockingBay, dockingBay.GetDockedVehicle(), true);

                skipPrefix = true;
                TARGET_METHOD.Invoke(context.Target, new[] { context.GuiHand });
                skipPrefix = false;
            }
            else
            {
                HandReticle.main.SetInteractText("Another player is using this vehicle!");
                HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                context.Target.isValidHandTarget = false;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
