using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class Vehicle_OnHandClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        private static Vehicle vehicle;
        private static GUIHand guiHand;
        private static bool skipPrefix = false;

        public static bool Prefix(Vehicle __instance, GUIHand hand)
        {
            if (skipPrefix)
            {
                return true;
            }
            
            vehicle = __instance;
            guiHand = hand;

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
            
            string guid = GuidHelper.GetGuid(__instance.gameObject);

            if (simulationOwnership.HasExclusiveLock(guid))
            {
                StaticLogger.Instance.Debug($"Already have an exclusive lock on the vehicle: {guid}");
                return true;
            }

            simulationOwnership.RequestSimulationLock(guid, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse);

            return false;
        }

        private static void ReceivedSimulationLockResponse(string guid, bool lockAquired)
        {
            if (lockAquired)
            {
                skipPrefix = true;
                TARGET_METHOD.Invoke(vehicle, new[] { guiHand });
                skipPrefix = false;
            }
            else
            {
                vehicle.gameObject.AddComponent<DenyOwnershipHand>();
                vehicle.isValidHandTarget = false;
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
