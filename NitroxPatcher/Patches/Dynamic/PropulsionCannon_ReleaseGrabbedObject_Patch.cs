using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PropulsionCannon_ReleaseGrabbedObject_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((PropulsionCannon t) => t.ReleaseGrabbedObject());

        public static bool Prefix(PropulsionCannon __instance)
        {
            GameObject grabbed = __instance.grabbedObject;
            if (!grabbed)
            {
                return false;
            }

            if (grabbed.TryGetIdOrWarn(out NitroxId id))
            {
                // Request to be downgraded to a transient lock so we can still simulate the positioning.
                Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);
            }

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
