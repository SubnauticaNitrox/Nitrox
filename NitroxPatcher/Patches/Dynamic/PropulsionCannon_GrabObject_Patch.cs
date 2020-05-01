using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PropulsionCannon_GrabObject_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PropulsionCannon);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("GrabObject", BindingFlags.Public | BindingFlags.Instance);

        private static NitroxId id;
        private static PropulsionCannon cannon;
        private static GameObject grabbedObject;

        private static bool skipPrefixPatch = false;

        public static bool Prefix(PropulsionCannon __instance, GameObject target)
        {
            if (skipPrefixPatch)
            {
                return true;
            }

            cannon = __instance;
            grabbedObject = target;

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            id = NitroxEntity.GetId(grabbedObject);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on the grabbed propulsion cannon object: {id}");
                return true;
            }

            simulationOwnership.RequestSimulationLock(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired)
        {
            if (lockAquired)
            {
                EntityPositionBroadcaster.WatchEntity(id, grabbedObject);

                skipPrefixPatch = true;
                cannon.GrabObject(grabbedObject);
                skipPrefixPatch = false;
            }
            else
            {
                grabbedObject.AddComponent<DenyOwnershipHand>();
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
