using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Vehicle_OnKill_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Vehicle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnKill", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(Vehicle __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            NitroxServiceLocator.LocateService<SimulationOwnership>().StopSimulatingEntity(id);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
