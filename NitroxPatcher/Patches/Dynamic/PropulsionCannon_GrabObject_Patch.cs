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
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PropulsionCannon_GrabObject_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PropulsionCannon);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("GrabObject", BindingFlags.Public | BindingFlags.Instance);

        private static bool skipPrefixPatch = false;

        public static bool Prefix(PropulsionCannon __instance, GameObject target)
        {
            if (skipPrefixPatch)
            {
                return true;
            }

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            NitroxId id = NitroxEntity.GetId(target);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on the grabbed propulsion cannon object: {id}");
                return true;
            }

            PropulsionGrab context = new PropulsionGrab(__instance, target);
            LockRequest<PropulsionGrab> lockRequest = new LockRequest<PropulsionGrab>(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

            simulationOwnership.RequestSimulationLock(lockRequest);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, PropulsionGrab context)
        {
            if (lockAquired)
            {
                EntityPositionBroadcaster.WatchEntity(id, context.GrabbedObject);

                skipPrefixPatch = true;
                context.Cannon.GrabObject(context.GrabbedObject);
                skipPrefixPatch = false;
            }
            else
            {
                context.GrabbedObject.AddComponent<DenyOwnershipHand>();
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
