using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD.Components;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PropulsionCannon_GrabObject_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PropulsionCannon t) => t.GrabObject(default(GameObject)));

        private static bool skipPrefixPatch;

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

            PropulsionGrab context = new(__instance, target);
            LockRequest<PropulsionGrab> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

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
