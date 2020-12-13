using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using UnityEngine;

namespace Nitrox.Patcher.Patches.Dynamic
{
    public class PropulsionCannon_ReleaseGrabbedObject_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PropulsionCannon);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ReleaseGrabbedObject", BindingFlags.Public | BindingFlags.Instance);
        
        public static bool Prefix(PropulsionCannon __instance)
        {
            GameObject grabbed = __instance.grabbedObject;

            if(!grabbed)
            {
                return false;
            }

            NitroxId id = NitroxEntity.GetId(grabbed);
            
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT, null);

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
